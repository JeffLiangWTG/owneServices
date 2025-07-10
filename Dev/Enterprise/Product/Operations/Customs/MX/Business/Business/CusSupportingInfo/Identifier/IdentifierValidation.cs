using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business
{
	public class IdentifierValidation : CusSupportingInfoValidation
	{
		public IdentifierValidation(Identifier parent) : base(parent)
		{
		}

		public new Identifier Parent => (Identifier)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			CheckComplementProperty(Parent.CSI_ReferenceNumberInfo, Parent.Complement1MaxLength, Parent.CSI_ReferenceNumber.Length);
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			CheckComplementProperty(Parent.CSI_ReferenceNumber2Info, Parent.Complement2MaxLength, Parent.CSI_ReferenceNumber2.Length);
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			CheckComplementProperty(Parent.CSI_DescriptionInfo, Parent.Complement3MaxLength, Parent.CSI_Description.Length);
		}

		void CheckComplementProperty(ZPropertyInfo propertyInfo, ZInt maxLength, ZInt propertyLength)
		{
			if (!propertyInfo.ReadOnly)
			{
				if (maxLength > 0 && propertyLength > maxLength)
				{
					propertyInfo.AddMessageError(Res.GetString("D4958AFB-6523-496E-9180-D2AF02587158", "{0} exceeded the max size {1}", propertyInfo.HumanReadableName, maxLength));
				}
			}
		}
	}
}
