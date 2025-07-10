using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.RemoteDeviceManagement;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceHeaderValidation : DmgDeviceHeaderValidation
	{
		public ClientDeviceHeaderValidation(AutoDmgDeviceHeader parent) : base(parent)
		{
		}

		protected override void CheckCDH_ModelID()
		{
			base.CheckCDH_ModelID();
			MandatoryValidation.CheckEntered(Parent.CDH_ModelIDInfo);
			if (Parent.CDH_IsTemplate)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CDH_ModelIDInfo, new ClientDeviceHeaderTemplateCollection(Parent.Factory));
			}
		}

		protected override void CheckCDH_Identifier()
		{
			base.CheckCDH_Identifier();
			if (Parent.CDH_Identifier.IsEmpty || Parent.CDH_IdentifierInfo.ReadOnly)
			{
				return;
			}

			var reg = new Regex(string.Format(CultureInfo.InvariantCulture, "^{0}\\d+$", ClientNumberFountainRegistration.TelematicsDevicePrefix));

			if (reg.IsMatch(Parent.CDH_Identifier))
			{
				Parent.CDH_IdentifierInfo.AddError(Res.GetString("FF2F5EBA-3FB0-4F9F-A64C-29ABC7710A61", "A manually entered device ID must not begin with the same prefix as the Telematics Device number fountain."));
			}

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CDH_IdentifierInfo, new ClientDeviceHeaderCollection(Parent.Factory));
		}

		protected override void CheckCDH_Description()
		{
			base.CheckCDH_Description();
			MandatoryValidation.CheckEntered(Parent.CDH_DescriptionInfo);
		}

		protected override void CheckCDH_EnterpriseCode()
		{
			base.CheckCDH_EnterpriseCode();
			ListValidation.ErrorIfInvalidCode(Parent.CDH_EnterpriseCodeInfo);
			ValidateDeviceHasMasterIdentifier(Parent.CDH_EnterpriseCodeInfo);
		}

		protected override void CheckCDH_ServerCode()
		{
			base.CheckCDH_ServerCode();
			ListValidation.ErrorIfInvalidCode(Parent.CDH_ServerCodeInfo);
			ValidateDeviceHasMasterIdentifier(Parent.CDH_ServerCodeInfo);
		}

		void ValidateDeviceHasMasterIdentifier(ZPropertyInfo propInfo)
		{
			var header = (ClientDeviceHeader)Parent;

			if (header.CDH_ServerCode.IsEmpty || header.CDH_EnterpriseCode.IsEmpty)
			{
				return;
			}

			if (header.CDH_DeviceIdentifier.IsEmpty)
			{
				propInfo.AddWarning(Res.GetString("34ECEA06-6606-41C8-A42C-2F6342636801", "This device has no hardware identifier. It will not be visible on client systems."));
			}
		}
	}
}

