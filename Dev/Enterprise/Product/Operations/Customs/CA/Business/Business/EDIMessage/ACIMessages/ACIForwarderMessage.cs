//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Edifact.D11B.Messages.GOVCBR;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business.CustomValues;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	[SystemDefinedValues]
	public class ACIForwarderMessage : EDIMessage, IControllerIDProvider
	{
		public ACIForwarderMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly TypeDecider TypeDecider = new ACIForwarderMessageTypeDecider();

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAACI).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAACI;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return EM_ReceiveTransmit == EDIInterchange.Direction.Receive ? ACIForwarderReceivedMessageSubTypeList : ACIForwarderMessageSubTypeList; }
		}

		CodeDescriptionPairList ACIForwarderMessageSubTypeList
		{
			get { return new ACIForwarderMessageTypes(); }
		}

		CodeDescriptionPairList ACIForwarderReceivedMessageSubTypeList
		{
			get { return new ACIForwarderReceivedMessageTypes(); }
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		#endregion

		#region Properties

		public GOVCBRMessage GOVCBR
		{
			get { return govcbr ?? (govcbr = (GOVCBRMessage)GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet())); }
		}
		GOVCBRMessage govcbr;

		public ZDateTime ProcessingDate
		{
			get { return this.GetSystemDefinedValue<ZDateTime>(Schema.RNSProcessingDate); }
			set
			{
				var oldValue = RNSProcessingDate;
				this.SetSystemDefinedValue(Schema.RNSProcessingDate, value);
				RNSProcessingDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZString BGMReference
		{
			get { return GOVCBR.BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		public override ZString ReferenceNumber
		{
			get { return BGMReference; }
		}
		public override ZDateTime RNSProcessingDate
		{
			get
			{
				ZDateTime dateTime = ZDateTime.Empty;
				if (GOVCBR != null)
				{
					ZDateTime.TryParseExact(GOVCBR.DTM[0].DateTimePeriod.DateOrTimeOrPeriodText, out dateTime, "yyyyMMddHHmm");
				}
				return dateTime;
			}
		}

		public override ZString StatusDescription
		{
			get
			{
				var wrapper = new EManifestResponseWrapper(this);
				if (wrapper.IsMatchedNotice)
				{
					return Res.GetString("33C9471A-4CC0-4461-903D-0025671CA63B", "MATCHED");
				}
				else if (wrapper.IsNOTMatchedNotice)
				{
					return Res.GetString("1CDC7150-4017-4913-A624-DAA6802FE461", "NOT MATCHED");
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.CA.K84Reports; }  //todo: replace with manifest forfawd module once created
		}

		#endregion
	}
}
