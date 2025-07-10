using CargoWise.EntityFramework;
namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceConnectionValidation : AutoLicenceConnectionValidation
	{
		public LicenceConnectionValidation(AutoLicenceConnection parent)
			: base(parent)
		{
		}

		#region LK_RemoteAccessMethod

		protected override void CheckLK_RemoteAccessMethod()
		{
			base.CheckLK_RemoteAccessMethod();
			ListValidation.ErrorIfInvalidCode(Parent.LK_RemoteAccessMethodInfo, Parent.Lookups.RemoteAccessMethodsList);
		}

		#endregion
	}
}

