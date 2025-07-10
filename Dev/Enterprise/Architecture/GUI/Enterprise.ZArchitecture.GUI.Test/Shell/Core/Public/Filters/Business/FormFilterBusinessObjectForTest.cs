using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class FormFilterBusinessObjectForTest : FormFilterBuisnessObject<DummyBusinessObject>
	{
		public new Dictionary<ZString, Func<ModuleFilter, DummyBusinessObject, bool>> FuncDictionary
		{
			get
			{
				return base.FuncDictionary;
			}
		}

		public FormFilterBusinessObjectForTest() : base(LayoutsTestDataHelper.TestModuleID)
		{
		}

		protected override void AddOrUpdateFunc(ModuleFilterCollection filters)
		{
			AddTextFunc(filters, FormFilterBusinessObjectConstants.Description, x => x.Z0_Description);
			AddTranslatableTextFunc(filters, FormFilterBusinessObjectConstants.DescriptionChild, x => x.Collection.Select(child => child.Z0_Description), (NoResString)"Child Description");
			AddNumberRangeFunc(filters, FormFilterBusinessObjectConstants.Decimal, x => x.Z0_Decimal);
		}
	}
}
