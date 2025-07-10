using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RCLShippingManifestImporter : ManifestImporter
	{
		public RCLShippingManifestImporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void ProcessLine(string line)
		{
			var oCsvLine = new OCsvLine(line);
			if (oCsvLine.FieldValues.Length == 1)
			{
				ProcessHeaderInfoLine(oCsvLine.FieldValues);
			}
			else if (oCsvLine.FieldValues.Length >= MinimumNumberOfFieldsInDataLine)
			{
				if (!hasHeadingLineBeenRead)
				{
					hasHeadingLineBeenRead = true;
				}
				else
				{
					ProcessContainerInfoLine(oCsvLine.FieldValues);
				}
			}
		}

		protected void ProcessHeaderInfoLine(string[] fieldValues)
		{
			if (GeneratedHeader.ED_ManifestType.IsEmpty)
			{
				GeneratedHeader.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			}

			if (GeneratedHeader.ED_TransportMode.IsEmpty)
			{
				GeneratedHeader.ED_TransportMode = Core.Constants.TransportModes.Sea;
			}
		}

		protected void ProcessContainerInfoLine(string[] fieldValues)
		{
			ZString voyage = fieldValues[VoyageIndex].Trim();
			if (GeneratedHeader.ED_VoyageNumber.IsEmpty && !voyage.IsEmpty)
			{
				GeneratedHeader.ED_VoyageNumber = voyage;
			}

			AddContainer(fieldValues[EmptyFullIndex].Trim(), fieldValues[CANExemptionIndex].Trim(), fieldValues[DescriptionIndex].Trim(), fieldValues[DestinationIndex].Trim());
		}

		protected void AddContainer(ZString emptyFullIndicator, ZString cANOrExemption, ZString description, ZString destination)
		{
			GeneratedHeader.ED_NoOfContainer++;
			if (emptyFullIndicator == "EMPTY")
			{
				GeneratedHeader.ED_NoOfEmptyContainers++;
			}
			else
			{
				AddFullContainer(cANOrExemption, description, destination);
			}
		}

		protected void AddFullContainer(ZString cANOrExemption, ZString description, ZString destination)
		{
			if (new CMRExportExemptionCodesList().ContainsCode(cANOrExemption) || (cANOrExemption.Length == 3 && cANOrExemption.StartsWith("EX")))
			{
				AddExemptContainer(CMRExportExemptionCodes.GetFromExit2Exemption(cANOrExemption), description, destination);
			}
			else
			{
				AddNonExemptContainer(cANOrExemption);
			}
		}

		protected void AddExemptContainer(ZString exemptionCode, ZString description, ZString destination)
		{
			ExportCustomsManifestLines newLine = GeneratedHeader.Lines.AddNew();
			newLine.EL_TypeOfCAN = exemptionCode;
			newLine.EL_NumberOfContainers = 1;
			newLine.EL_GoodsDescription = description;
			newLine.EL_RN_NKCountryOfDestination = destination.SubstringSafe(0, 2);
		}

		protected void AddNonExemptContainer(ZString cAN)
		{
			ExportCustomsManifestLines line = GetOrCreateLine(cAN);
			line.EL_NumberOfContainers++;
		}

		protected ExportCustomsManifestLines GetOrCreateLine(ZString cAN)
		{
			foreach (ExportCustomsManifestLines line in GeneratedHeader.Lines)
			{
				if (line.EL_CAN == cAN)
				{
					return line;
				}
			}
			ExportCustomsManifestLines newLine = GeneratedHeader.Lines.AddNew();
			newLine.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			newLine.EL_CAN = cAN;
			return newLine;
		}

		protected sealed override BillingInterfaceName InterfaceName
		{
			get { return BillingInterfaceName.RCLShippingManifestImport; }
		}

		protected const int VoyageIndex = 1;
		protected const int EmptyFullIndex = 12;
		protected const int CANExemptionIndex = 17;
		protected const int DescriptionIndex = 21;
		protected const int DestinationIndex = 7;

		protected bool hasHeadingLineBeenRead;
		protected const int MinimumNumberOfFieldsInDataLine = 26;
	}
}
