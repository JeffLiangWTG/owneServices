using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Common.GUI
{
	public abstract class TariffColumnStyle : ZCodeFindBoxColumnStyle
	{
		protected TariffColumnStyle(Func<TariffGridFindBox> findBox, TariffColumnStyleInfo info)
			: base(findBox, info)
		{
		}

		public new TariffGridFindBox FindBox
		{
			get { return (TariffGridFindBox)base.FindBox; }
		}

		protected override void InitialiseEditControlForNewPosition(BusinessObject bizObj, bool readOnly)
		{
			base.InitialiseEditControlForNewPosition(bizObj, readOnly);
			FindBox.ActiveBusinessObject = bizObj;
			FindBox.ColumnInfo = ColumnInfo;
		}
	}

	public abstract class TariffColumnStyleInfo : ZCodeFindBoxColumnStyleInfo, ICustomModuleFilterProvider
	{
		public ModuleFilter GetModuleFilter(SchemaColumn columnSchema, string headerText)
		{
			if (columnSchema is SchemaStringColumn tariffSchemaColumn)
			{
				return new ModuleNumberFilter(ColumnName, tariffSchemaColumn) { MultilingualDescription = (NoResString)headerText };
			}
			throw new ArgumentException("TariffColumnStyle should only be applied to ZString column, but applied to " + columnSchema.Name + ", which is type of " + columnSchema.GetType(), nameof(columnSchema));
		}
	}
}
