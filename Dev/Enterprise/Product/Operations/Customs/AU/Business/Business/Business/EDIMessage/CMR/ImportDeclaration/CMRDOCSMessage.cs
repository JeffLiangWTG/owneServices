using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDOCSMessage : CMRImportDeclarationMessage
	{
		public CMRDOCSMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.DOCS;
		}

		protected override ZString GetStatusCore()
		{
			return GetReferenceFromSendersReference(SendersReference);
		}

		public override ZString GetReport()
		{
			StringBuilder report = new StringBuilder();
			report.Append(base.GetReport());
			report.Append(GetReasons());
			report.Append(GetCustomsOfficerInforamtion());
			return report.ToString();
		}

		public override ZString StatusOfLinesReport
		{
			get { return ZString.Empty; }
		}

		ZString GetReasons()
		{
			ZString result = ZString.Empty;

			if (CUSRES.FTX.Count > 0)
			{
				result += "\r\nReason:\r\n";

				foreach (FTXSegment fTX in CUSRES.FTX)
				{
					result += "\t" + fTX.TextLiteral.FreeTextValue1 + " " + fTX.TextLiteral.FreeTextValue2
						+ fTX.TextLiteral.FreeTextValue3 + " " + fTX.TextLiteral.FreeTextValue4 + "\r\n";
				}
			}

			return result;
		}

		ZString GetCustomsOfficerInforamtion()
		{
			ZString result = ZString.Empty;

			foreach (SegmentGroup1 group1 in CUSRES.Group1)
			{
				foreach (NADSegment nAD in group1.NAD)
				{
					if (nAD.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Customs)
					{
						result += "\r\nCustoms Officer:\r\n";
						result += "\tName: " + nAD.NameAndAddress.NameAndAddressLine1 + "\r\n";
					}
				}

				foreach (SegmentGroup2 group2 in group1.Group2)
				{
					foreach (COMSegment cOM in group2.COM)
					{
						ZString cOMValue = cOM.CommunicationContact.CommunicationNumberCodeQualifier.ToString();

						if (cOMValue == CommunicationNumberCodeQualifierList.Telephone)
						{
							result += "\tPhone: " + cOM.CommunicationContact.CommunicationNumber + "\r\n";
						}

						if (cOMValue == CommunicationNumberCodeQualifierList.ElectronicMail)
						{
							result += "\tEmail: " + cOM.CommunicationContact.CommunicationNumber.ToLower() + "\r\n";
						}

						if (cOMValue == CommunicationNumberCodeQualifierList.Telefax)
						{
							result += "\tFax: " + cOM.CommunicationContact.CommunicationNumber + "\r\n";
						}
					}
				}
			}

			return result;
		}
	}
}
