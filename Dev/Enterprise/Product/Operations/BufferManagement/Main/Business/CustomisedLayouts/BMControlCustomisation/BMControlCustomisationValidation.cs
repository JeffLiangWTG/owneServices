using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationValidation : AutoBMControlCustomisationValidation
	{
		public BMControlCustomisationValidation(AutoBMControlCustomisation parent)
			: base(parent)
		{
		}

		new BMControlCustomisation Parent
		{
			get { return (BMControlCustomisation)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWidth();
			ValidateHeight();
			ValidateBackgroundColor();
		}

		protected override void CheckFM_Name()
		{
			base.CheckFM_Name();
			MandatoryValidation.CheckEntered(Parent.FM_NameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FM_NameInfo, Parent.Factory.Load<BMControlCustomisation>(new ZQuery(BMControlCustomisationSchema.FM_ControlType, Parent.FM_ControlType)));
		}

		protected override void CheckFM_ControlType()
		{
			base.CheckFM_ControlType();
			MandatoryValidation.CheckEntered(Parent.FM_ControlTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FM_ControlTypeInfo);
		}

		protected override void CheckFM_IsSystemWide()
		{
			base.CheckFM_IsSystemWide();

			if (Parent.FM_IsSystemWide)
			{
				var query = new ZQuery(BMControlCustomisationSchema.FM_ControlType, Parent.FM_ControlType);
				query.AddToFilter(BMControlCustomisationSchema.FM_IsSystemWide, true);
				query.AddToFilter(BMControlCustomisationSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Exists(typeof(BMControlCustomisation), query, mergeDbAndCacheResult: false))
				{
					Parent.FM_IsSystemWideInfo.AddError(Res.GetString("7f16d149-d0a3-46b3-80f2-94adabb4c3c9", "Only one system-wide customization may be made per Control Type."));
				}
			}
		}

		public void ValidateWidth()
		{
			ValidateCalculatedProperty(Parent.WidthInfo);
		}

		protected void CheckWidth()
		{
			CompareValidation.CheckWithinRange(Parent.WidthInfo, 25, 800);
		}

		public void ValidateHeight()
		{
			ValidateCalculatedProperty(Parent.HeightInfo);
		}

		protected void CheckHeight()
		{
			CompareValidation.CheckWithinRange(Parent.HeightInfo, 25, 600);
		}

		public void ValidateBackgroundColor()
		{
			ValidateCalculatedProperty(Parent.BackgroundColorInfo);
		}

		protected void CheckBackgroundColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BackgroundColorInfo);
		}
	}
}
