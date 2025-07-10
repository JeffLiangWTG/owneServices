using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Macros;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuItemValidation : StmMenuItemBaseValidation
	{
		public VisualizerMenuItemValidation(VisualizerMenuItem parent)
			: base(parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
		}

		readonly VisualizerMenuItem parent;

		protected override void CheckSU_PrimaryDocPackItemId()
		{
			base.CheckSU_PrimaryDocPackItemId();

			if (!parent.SU_PrimaryDocPackItemIdInfo.HasErrors()
				&& parent.Documents.Count > 1
				&& (!parent.SU_PrimaryDocPackItemId.IsValid || parent.Documents.All(doc => doc.PK != parent.SU_PrimaryDocPackItemId)))
			{
				parent.SU_PrimaryDocPackItemIdInfo.AddError(Res.GetString("2a3dfc8d-280a-4de1-82d2-3d9815710909", "Since you have more than one document, you have to select a primary document."));
			}
		}

		protected override void CheckSU_DeliveryRestrictionMacro()
		{
			base.CheckSU_DeliveryRestrictionMacro();

			if (parent.SU_DeliveryRestrictionType == nameof(DeliveryRestrictionType.UDF) && !string.IsNullOrEmpty(Parent.SU_DeliveryRestrictionMacro))
			{
				var expr = parent.SU_DeliveryRestrictionMacro.ToString()
					.With<StandardLibrary>()
					.CreateExpression();

				if (expr.Compile() is ErrorMessage errorMessage)
				{
					parent.SU_DeliveryRestrictionMacroInfo.AddError(errorMessage.Message);
				}
			}
		}

		protected override void CheckSU_EmailSubjectLine()
		{
			base.CheckSU_EmailSubjectLine();

			var validationErrors = EmailSubjectMacroValidator.Validate(Parent.SU_EmailSubjectLine);

			if (!string.IsNullOrEmpty(validationErrors))
			{
				parent.SU_EmailSubjectLineInfo.AddError(validationErrors);
			}
		}

		IEmailSubjectMacroValidator EmailSubjectMacroValidator => emailSubjectMacroValidator ?? (emailSubjectMacroValidator = ObjectFactory.Get<IEmailSubjectMacroValidator>());
		IEmailSubjectMacroValidator emailSubjectMacroValidator;

		public override void ValidateAll()
		{
			parent.ClearRowNotifications();
			base.ValidateAll();
			parent.ValidateFilterExpression(parent.SU_FilterList);
		}
	}
}
