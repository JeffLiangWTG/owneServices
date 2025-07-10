using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class GlbExternalPasswordValidation : GlbExternalPasswordWithCertificateValidation
	{
		public GlbExternalPasswordValidation(GlbExternalPassword parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword Parent => (GlbExternalPassword)base.Parent;

		protected override void CheckGP_Name()
		{
			base.CheckGP_Name();
			var name = Parent.GP_Name;
			if (name.IsEmpty)
			{
				Parent.GP_NameInfo.AddError(Res.GetString("449497F1-9091-4258-8E61-FE5380665D0A", "The certificate name must not be empty."));
			}
			else
			{
				if (ParentCollection?.Cast<GlbExternalPassword>().Any(sub => sub.PK != Parent.PK && sub.GP_Name.EqualsIgnoringCase(name)) ?? false)
				{
					Parent.GP_NameInfo.AddError(Res.GetString("4E8B48B3-0807-4DDE-BF6F-CA60E8D703D0", "The certificate name has already been registered, it must be unique."));
				}
			}
		}
		protected override void CheckGP_IssueDateIsValidZDateTimeRange() { }

		GlbExternalPasswordCollection ParentCollection => GlbStaffWrapper.Get(Parent.Staff)?.ESBPasswordCollection;
	}
}
