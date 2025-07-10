using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	/// <summary>
	/// Module Controller for JobDocAddresses.
	/// </summary>
	public class JobDocAddressesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public JobDocAddressesController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CAJobDocAddresses; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CAJobDocAddresses; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDocAddress); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			SetInitialTabPageNameToSelectWhenAFormIsShown(OrganisationTabPages.Address.Name);
			JobDocAddress address = (JobDocAddress)businessEntity;
			return new JobDeclarationForm(address.Parent as JobDeclaration);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CAJobDocAddressesView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CAJobDocAddressesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CAJobDocAddressesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CAJobDocAddressesNew; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.DocAddresses.AddNew();
		}
	}
}
