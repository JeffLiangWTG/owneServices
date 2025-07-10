using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationPackageValidation : ZValidation
	{
		public ExitNotificationPackageValidation(ExitNotificationPackage parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly ExitNotificationPackage parent;

		public override Type AutoValidationType => typeof(ExitNotificationPackageValidation);

		public override void ValidateAll()
		{
			ValidatePackQTY();
		}

		public void ValidatePackQTY() => ValidateCalculatedProperty(parent.PackQTYInfo);

		protected void CheckPackQTY()
		{
			var packQTY = parent.PackQTY;

			if (packQTY < 0 || packQTY > 99_999_999u)
			{
				parent.PackQTYInfo.AddError(Res.GetString("48D4D6C6-28D2-46AE-8F72-F98DE9992338", "Package Quantity {0} must not be less than 0 or greater than 99.999.999", packQTY));
			}
		}
	}
}
