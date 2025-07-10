using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public abstract class DocumentBrandingBusinessObject : ClientAndAgentBrandingBusinessObject
	{
		public DocumentBrandingBusinessObject()
		{
		}

		public DocumentBrandingBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Override validation

		public override void ValidateBrandName()
		{
			BrandNameInfo.ClearAllNotifications();
			if (BrandName.Trim().IsEmpty)
			{
				BrandNameInfo.AddError(Res.GetString("9f6421da-d771-43b9-8fa8-27ad56ac4107", "Please enter a Brand Name."));
			}
		}

		public override void ValidateBrandEmailAddress()
		{
			BrandEmailAddressInfo.ClearAllNotifications();
			if (BrandEmailAddress.Trim().IsEmpty)
			{
				BrandEmailAddressInfo.AddError(Res.GetString("d905a6f9-d3e0-41af-a121-b45124c7e613", "Please enter a valid email address."));
			}
			else
			{
				EmailAddressValidation.ValidateEmailAddress(BrandEmailAddressInfo);
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ValidateBrandEmailAddress();
			ValidateBrandName();
			base.RunPreSaveValidationCore();
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.BrandName, BrandName);
			writer.WriteElementString(Schema.BrandEmailAddress, BrandEmailAddress);
			writer.WriteElementString(Schema.UseGeneric, UseGeneric.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			BrandName = reader.ReadElementString(Schema.BrandName);
			BrandEmailAddress = reader.ReadElementString(Schema.BrandEmailAddress);
			UseGeneric = new ZBool(reader.ReadElementString(Schema.UseGeneric));
		}
	}
}
