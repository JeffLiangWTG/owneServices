using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public abstract class CACFilterGridModule : ZFilterGridModule
	{
		public override ZBool HasActions
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			Globals.Message.Show(Res.GetString("e64fbb9f-1648-4b39-8bcc-219b0f521518", "There is no information to show for the selected record"));
			return null;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}
	}
}
