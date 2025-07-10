using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryInstructionCollection : EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>
	{
		public CusEntryInstructionCollection(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.RefreshBinding();
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var cei = child as CusEntryInstruction;
			if (cei != null)
			{
				using (cei.SuspendSettingHasChanges())
				{
					cei.DefaultPackageCount();
				}
			}

			base.SetDefaultsForNewChild(child);
		}
	}
}
