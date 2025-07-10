using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.TNT.DocWrappers
{
	public class TNTHawb : DocAWB
	{
		#region Constructors and Type override

		protected TNTHawb(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
			: base(exportAWBHeader, factoryToWrap)
		{
		}

		public new static TNTHawb New(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
		{
			return (exportAWBHeader == null) ? null : new TNTHawb(exportAWBHeader, factoryToWrap);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public ZString AccountOrganisationName
		{
			get { return OrganisationBasedOnPrePaidOrCollect != null ? "A/C: " + OrganisationBasedOnPrePaidOrCollect.Name : ""; }
		}

		public ZString AccountNumber
		{
			get { return OrganisationBasedOnPrePaidOrCollect != null ? GetRegistrationCodeThatIsLSCType(OrganisationBasedOnPrePaidOrCollect.CustomCodes) : ZString.Empty; }
		}

		public ZString HouseBill
		{
			get
			{
				ZString result = ZString.Empty;
				if (ShipmentExportAWBHeader != null)
				{
					result = ShipmentExportAWBHeader.Shipment.JS_HouseBill;
				}
				return result;
			}
		}

		#region Implementation

		DocOrganisation OrganisationBasedOnPrePaidOrCollect
		{
			get
			{
				DocOrganisation result = null;
				if (ShipmentExportAWBHeader != null && ShipmentExportAWBHeader.Shipment != null && JobHeader != null)
				{
					result = ShipmentExportAWBHeader.Shipment.IsPrepaid ? JobHeader.LocalCharges : JobHeader.AgentCollect;
				}
				return result;
			}
		}

		ZString GetRegistrationCodeThatIsLSCType(DocCusCodeCollection cusCodes)
		{
			ZString result = ZString.Empty;
			foreach (DocCusCode cusCode in cusCodes)
			{
				if (cusCode.CodeType == OrgCusCode.CodeTypes.LegacySystemCode)
				{
					result = DelimitCharactersWithSpace(cusCode.CustomsRegNo);
					break;
				}
			}
			return result;
		}

		ZString DelimitCharactersWithSpace(ZString stringValue)
		{
			ZString result = ZString.Empty;
			foreach (char character in stringValue)
			{
				result += character + " ";
			}
			return result.Trim();
		}

		#endregion
	}
}
