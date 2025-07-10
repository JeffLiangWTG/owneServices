using CargoWise.Common;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRuleOptionCollection : CodeDescriptionOptionCollection
	{
		public MergingRuleOptionCollection(JobDeclaration declaration) : base(new CodeDescriptionOptionCollectionParent(declaration.Factory, declaration.MergingRules))
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));

			SetReadOnlyIncludingChildren(this.declaration.NoMerge);
			this.declaration.JE_MergeByInfo.ValueChanged += JE_MergeByInfo_ValueChanged;
		}

		void JE_MergeByInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var noMerge = declaration.NoMerge;
			if (noMerge)
			{
				UnselectAll();
			}
			SetReadOnlyIncludingChildren(noMerge);
		}

		readonly JobDeclaration declaration;
	}
}
