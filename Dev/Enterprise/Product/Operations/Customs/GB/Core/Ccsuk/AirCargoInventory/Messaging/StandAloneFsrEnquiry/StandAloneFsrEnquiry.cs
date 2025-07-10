using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class StandAloneFsrEnquiry : EDIMessage
	{
		public StandAloneFsrEnquiry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static StandAloneFsrEnquiry MakeNewOutboundFromPayload(NonPersistentStandAloneFsrEnquiryForNew npbo)
		{
			var awbNumber = npbo.AwbNumberFormatted;
			var edifact = new CukFsrCreator(npbo, npbo.AwbNumberFormatted, null, npbo.Factory).MakeMessageText();
			var persistentMessage = npbo.Factory.New<StandAloneFsrEnquiry>();
			persistentMessage.EM_MessageOwner = npbo.PIMA;
			persistentMessage.EM_Status = EDIMessage.Status.Queued;
			persistentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			persistentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(npbo.Factory, ApplicationCodeList.Codes.GbCcsuk);
			persistentMessage.EM_MessageText = edifact;
			persistentMessage.EM_ApplicationReference = (npbo.Airport + npbo.Shed + " " + awbNumber).Trim();
			persistentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			persistentMessage.EM_MessageInterpretation = string.Format(@"<html>
<body style='font-family: arial;'>
<h3>Community Database Enquiry (FSR)</h3>
<p>AWB: {0}</p>
<p>Airport/Shed: {1}{2}</p>
<p>PIMA: {3}</p>
<p>Database: {4}</p>
</body>
</html>",
					awbNumber,
					npbo.Airport, npbo.Shed,
					npbo.PIMA,
					new FsrRequestType().GetDescriptionFromCode(npbo.DatabaseToQuery)
					);

			return persistentMessage;
		}

		public override void OnSaving()
		{
			if (MessageNumberStrategy == null)
			{
				MessageNumberStrategy = new GbMessageNumberStrategy(Factory, ApplicationCodeList.Codes.GbCcsuk);
			}
			base.OnSaving();
			EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(EM_MessageText, this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode;
		}

		public StandAloneFsrEnquiry LinkedMessage
		{
			get { return HasLinkedMessage ? Factory.Load<StandAloneFsrEnquiry>(EM_LinkUniqueID) : null; }
		}

		public ZBool HasLinkedMessage
		{
			get { return !EM_LinkUniqueID.IsEmpty && EM_LinkTable == EDIMessage.Schema.TableName; }
		}

		[BusinessObjectTestExclude] // the test is flawed, it tries to set 101 characters, does NOT fail, and then the test fails because there is no exception thrown.
		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]  // 100?!
		public ZString ResponseText
		{
			get
			{
				var result = GenAddOn.XA_Data;
				if (result.IsEmpty)
				{
					// The summary text will be blank even when we have a response if the response reports multiple hits (e.g. bill is at two different sheds)
					result = HasLinkedMessage ? " ** Please open the record to see the full response ** " : "No reply has been received yet";
				}
				return result;
			}
			set
			{
				if (value.IsEmpty)
				{
					if (cachedAddOn != null && cachedAddOn.Value != null)
					{
						GenAddOn.Delete();
					}
				}
				else
				{
					var valueToSet = value.ToUpper().Left(GenAddOn.XA_DataInfo.MaxLength);
					CheckMaximumLength(ResponseTextInfo, valueToSet);
					GenAddOn.XA_Data = valueToSet;
				}
				ResponseTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ResponseTextInfo
		{
			get { return GetZPropertyInfo(nameof(ResponseText)); }
		}

		GenAddOnColumn GenAddOn => Factory.GetValue(ref cachedAddOn, () => FindOrMakeNewAddOn());

		CachedProperty<GenAddOnColumn> cachedAddOn;

		GenAddOnColumn FindOrMakeNewAddOn()
		{
			var addOnStatusQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, PK);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode);
			var result = Factory.LoadTop1<GenAddOnColumn>(addOnStatusQuery);
			if (result == null)
			{
				result = Factory.New<GenAddOnColumn>();
				result.XA_Name = CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode;
				result.XA_ParentTableCode = EDIMessageSchema.Constants.Prefix;
				result.XA_ParentID = PK;
			}
			return result;
		}

		public NonPersistentStandAloneFsrEnquiryForNew GetRequeryMessage()
		{
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(Factory);
			try
			{
				var shedAndAwb = EM_ApplicationReference.Split(' ');  // e.g. "LHRBAC 125-12345678", "LHRBAC 125-12345678-HOUSE001", "LHRBAC 125-12345678/01", "LHRBAC 125-12345678-HOUSE001/01", "125-12345678", "125-12345678-HOUSE001", etc
				var awbNumber = ZString.Empty;
				if (shedAndAwb.Length == 2)
				{
					var shedAndAirport = new ZString(shedAndAwb[0]);
					npbo.Airport = shedAndAirport.Left(3);
					npbo.Shed = shedAndAirport.Right(3);
					awbNumber = shedAndAwb[1];
				}
				else if (shedAndAwb.Length == 1)
				{
					awbNumber = shedAndAwb[0];
				}
				var splitParts = awbNumber.Split(new char[] { '/' });
				var awbParts = splitParts[0].Split(new char[] { '-' });
				npbo.MAWB = awbParts.Length > 1 ? awbParts[0] + awbParts[1] : string.Empty;
				npbo.HAWB = awbParts.Length > 2 ? awbParts[2] : ZString.Empty;
				npbo.SRF = splitParts.Length > 1 ? splitParts[1] : ZString.Empty;
				var databaseSearchString = string.Format(@"\{0}(?<DATABASE>(\w\w\w))\{1}(UNT|LOC|COM)", CharacterSet.ElementDelimiter, CharacterSet.SegmentDelimiter);  //e.g. +IMP'UNT 
				var rego = new Regex(databaseSearchString);
				var matches = rego.Matches(EM_MessageText);
				if (matches != null & matches.Count > 0 && matches[0].Groups.Count > 1 && matches[0].Groups[1].Captures.Count > 0)
				{
					npbo.DatabaseToQuery = matches[0].Groups[1].Captures[0].Value;
				}
				npbo.PIMA = Interchange.EI_From.Left(CcsukConstants.PimaMaxLength);
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }
			return npbo;
		}
	}
}
