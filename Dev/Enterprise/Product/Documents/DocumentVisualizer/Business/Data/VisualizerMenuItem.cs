using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.DocumentEngine.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuItem : StmMenuItemBase
	{
		public VisualizerMenuItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override CodeDescriptionPairList GetIncludedDocumentsList()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(Factory.Load<VisualizerMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, PK)).OrderBy(p => p.SI_Index).ToArray());

			return list;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SU_MenuType = Constants.StmMenuItemTypes.Forms;
			SU_PreventAutoDelivery = false;
		}

		public override CodeDescriptionPairList MenuTypeList
		{
			get
			{
				return Factory.GetCachedValue("VisualizerMenuItem.MenuTypeList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.StmMenuItemTypes.Forms, Res.GetString("c9b34771-dcef-4286-ae48-978a3eee81a2", "Visualizer Form"));

					return result;
				});
			}
		}

		protected override StmMenuTemplatePivotBaseCollection GetNewDocuments()
		{
			return new VisualizerMenuTemplatePivotCollection(Factory,
				new ZQuery(Enterprise.ZArchitecture.Schema.StmMenuTemplatePivotSchema.SI_SU, PK));
		}

		protected override bool GetPropertyInfosReadOnly(PropertyDescriptor property)
		{
			if (GlbStaff.CurrentUser.GS_IsDeveloper && GlbStaff.CurrentUser.GS_IsSystemAccount && IsAnyTemplateCheckedOutByMe)
			{
				return false;
			}

			var result = false;

			if (property != null &&
				(
					property.Name == StmMenuItemSchema.Constants.SU_DeliveryRestrictionType ||
					property.Name == StmMenuItemSchema.Constants.SU_DeliveryRestrictionMacro ||
					property.Name == StmMenuItemSchema.Constants.SU_DeliveryRestrictionDescription ||
					property.Name == StmMenuItemSchema.Constants.SU_EmailSubjectLine
				))
			{
				result = MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}
			else
			{
				result = base.GetPropertyInfosReadOnly(property);
			}

			return result;
		}

		bool SU_DeliveryRestrictionType_ReadOnly
		{
			get { return !Env.Security.ReceivablesOnCreditHoldController.IsAllowed && base.GetPropertyInfosReadOnly(null); }
		}

		protected override bool SU_DeliveryRestrictionMacro_ReadOnly
		{
			get { return SU_DeliveryRestrictionType_ReadOnly || base.SU_DeliveryRestrictionMacro_ReadOnly; }
		}

		protected override bool SU_DeliveryRestrictionDescription_ReadOnly
		{
			get { return SU_DeliveryRestrictionType_ReadOnly || base.SU_DeliveryRestrictionDescription_ReadOnly; }
		}

		#endregion

		#region Validation

		protected override StmMenuItemValidation GetNewValidation()
		{
			return new VisualizerMenuItemValidation(this);
		}

		#endregion

		bool IsAnyTemplateCheckedOutByMe => Documents.OfType<VisualizerMenuTemplatePivot>().Any(pivot => pivot.Template?.IsCheckedOutByMe ?? false);
	}
}
