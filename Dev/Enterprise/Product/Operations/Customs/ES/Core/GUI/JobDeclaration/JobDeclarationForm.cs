using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class JobDeclarationForm : EU.GUI.JobDeclarationForm
	{
		public JobDeclarationForm() { }

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			var preSaveStrategies = new List<PreSaveDialogStrategy>(base.GetPreSaveDialogStrategies());

			var declaration = Declaration;
			if (declaration != null)
			{
				preSaveStrategies.Add(new AEODocumentPreSaveDialogStrategy((JobDeclaration)declaration));
				preSaveStrategies.Add(new Document9015PreSaveDialogStrategy((JobDeclaration)declaration));
			}
			return preSaveStrategies;
		}

		protected override void HandleSaveException(Exception ex)
		{
			((JobDeclaration)Declaration).ReportSaveExceptionForSupplierOrImporter(ex);
			base.HandleSaveException(ex);
		}

		public void SetLocationOfGoodsUserControlReadOnly()
		{
			var locationOfGoodsUserControl = this.FindSingleOrDefault<LocationOfGoodsUserControl>("LocationOfGoodsUserControl");
			if (locationOfGoodsUserControl == null) { return; }

			var currentDataItem = locationOfGoodsUserControl.CurrentDataItem;
			if (currentDataItem == null || currentDataItem is not CusEntryInstruction) { return; }

			((CusEntryInstruction)currentDataItem).SetGoodsLocationReadOnly();
		}
	}
}
