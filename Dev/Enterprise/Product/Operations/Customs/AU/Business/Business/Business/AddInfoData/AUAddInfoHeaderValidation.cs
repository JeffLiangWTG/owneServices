using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUAddInfoHeaderValidation : AUAddInfoValidation
	{
		public AUAddInfoHeaderValidation(AutoAUAddInfo parent)
			: base(parent)
		{
		}

		#region Read Only Fields

		protected override void CheckZA_DMP()
		{
			base.CheckZA_DMP();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DMPInfo);
		}

		protected override void CheckZA_DRE()
		{
			base.CheckZA_DRE();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DREInfo);
		}

		protected override void CheckZA_DSN()
		{
			base.CheckZA_DSN();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DSNInfo);
		}

		protected override void CheckZA_DXP()
		{
			base.CheckZA_DXP();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DXPInfo);
		}

		protected override void CheckZA_DTY()
		{
			base.CheckZA_DTY();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DTYInfo);
		}

		protected override void CheckZA_ICN()
		{
			base.CheckZA_ICN();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_ICNInfo);
		}

		protected override void CheckZA_ADJ()
		{
			base.CheckZA_ADJ();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_ADJInfo);
		}

		protected override void CheckZA_LCT()
		{
			base.CheckZA_LCT();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_LCTInfo);
		}

		protected override void CheckZA_ISS()
		{
			base.CheckZA_ISS();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_ISSInfo);
		}

		protected override void CheckZA_LCTQ()
		{
			base.CheckZA_LCTQ();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_LCTQInfo);
		}

		protected override void CheckZA_LCTE()
		{
			base.CheckZA_LCTE();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_LCTEInfo);
		}

		protected override void CheckZA_MLP()
		{
			base.CheckZA_MLP();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_MLPInfo);
		}

		protected override void CheckZA_ODF()
		{
			base.CheckZA_ODF();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_ODFInfo);
		}

		protected override void CheckZA_RNO()
		{
			base.CheckZA_RNO();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_RNOInfo);
		}

		protected override void CheckZA_MD2()
		{
			base.CheckZA_MD2();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_MD2Info);
		}

		protected override void CheckZA_TC2()
		{
			base.CheckZA_TC2();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_TC2Info);
		}

		protected override void CheckZA_QT2()
		{
			base.CheckZA_QT2();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_QT2Info);
		}

		protected override void CheckZA_UQ2()
		{
			base.CheckZA_UQ2();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_UQ2Info);
		}

		protected override void CheckZA_STD()
		{
			base.CheckZA_STD();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_STDInfo);
		}

		protected override void CheckZA_TAN()
		{
			base.CheckZA_TAN();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_TANInfo);
		}

		protected override void CheckZA_TFQ()
		{
			base.CheckZA_TFQ();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_TFQInfo);
		}

		protected override void CheckZA_WRQ()
		{
			base.CheckZA_WRQ();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WRQInfo);
		}

		protected override void CheckZA_WRU()
		{
			base.CheckZA_WRU();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WRUInfo);
		}

		protected override void CheckZA_WUV()
		{
			base.CheckZA_WUV();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WUVInfo);
		}

		protected override void CheckZA_WET()
		{
			base.CheckZA_WET();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WETInfo);
		}

		protected override void CheckZA_WETQ()
		{
			base.CheckZA_WETQ();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WETQInfo);
		}

		protected override void CheckZA_WETE()
		{
			base.CheckZA_WETE();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WETEInfo);
		}

		#endregion

		/// <summary>
		/// Validate the properties invalid for header level that have non-default values.
		/// </summary>
		protected void ValidateLineLevelPropertyDoesntHaveNonDefaultValue(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsDefault)
			{
				if (!AddInfo.LineLevelAddInfo && AddInfo.IsALineLevelProperty(propertyInfo.Name))
				{
					propertyInfo.AddMessageError(propertyInfo.Name.Substring(3) + " is not a valid field for invoice headers. Please enter this value for each invoice line.");
				}
			}
		}
	}
}
