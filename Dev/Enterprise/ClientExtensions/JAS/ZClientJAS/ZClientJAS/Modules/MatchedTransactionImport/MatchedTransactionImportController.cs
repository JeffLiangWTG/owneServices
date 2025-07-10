using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.Client.JAS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class MatchedTransactionImportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ImportMatchedTransactions; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MatchedDataImporter); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			MatchedTransactionsImporter importer = new MatchedTransactionsImporter();
			return importer.ImporterForm;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new MatchedDataImporter();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
