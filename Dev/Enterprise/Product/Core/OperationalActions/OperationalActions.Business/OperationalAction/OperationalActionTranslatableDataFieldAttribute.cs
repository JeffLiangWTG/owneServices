using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class OperationalActionTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public OperationalActionTranslatableDataFieldAttribute(string columnName)
			: base(OperationalAction.Schema.TableName, columnName, DataXmlFilePaths.Documents)
		{
			Type = typeof(OperationalAction);
		}

		protected override string KeyPrefixContextColumnName
		{
			get { return base.KeyPrefixContextColumnName + "_" + Core.Constants.StmMenuItemTypes.OperationalActions; }
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = new ZQuery(StmMenuItemSchema.SU_MenuType, SQLComparisonOperator.Equal, Core.Constants.StmMenuItemTypes.OperationalActions);
				return filter;
			}
		}
	}
}
