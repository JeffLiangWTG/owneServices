using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuTemplatePivotValidation : StmMenuTemplatePivotBaseValidation
	{
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public VisualizerMenuTemplatePivotValidation(VisualizerMenuTemplatePivot parent)
			: base(parent)
		{
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
		}

		readonly VisualizerMenuTemplatePivot parent;

		protected override void CheckSI_DataStoreName()
		{
			base.CheckSI_DataStoreName();

			MandatoryValidation.CheckEntered(parent.SI_DataStoreNameInfo);

			if (!parent.SI_DataStoreName.IsEmpty)
			{
				var sharedStorageDocs = GetSharedStorageDocs().Where(x => x.MenuItem != null).ToArray();

				if (sharedStorageDocs.Length > 0)
				{
					var warningMessage = Res.GetString("42deeb2f-1327-406e-b968-c5b9e36b85c5", @"There are Forms in other Menu Items with the same Data Store Name, this means they will share data overrides. The other Forms with this Data Store Name are:
{0}", string.Join(System.Environment.NewLine, sharedStorageDocs.Select(GetFullPivotPathFormatted).OrderBy(x => x)));

					parent.SI_DataStoreNameInfo.AddWarning(warningMessage);
				}
			}
		}

		VisualizerMenuTemplatePivot[] GetSharedStorageDocs()
		{
			var businessContext = parent.MenuItem?.SU_BusinessContext;

			if (string.IsNullOrWhiteSpace(businessContext))
			{
				return Array.Empty<VisualizerMenuTemplatePivot>();
			}

			var query = new ZQuery(StmMenuTemplatePivotSchema.SI_DataStoreName, parent.SI_DataStoreName);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, SQLComparisonOperator.NotEqual, parent.SI_SU);

			return parent
				.Factory
				.Load<VisualizerMenuTemplatePivot>(query)
				.Where(p => string.CompareOrdinal(p.MenuItem?.SU_BusinessContext, businessContext) == 0)
				.ToArray();
		}

		string GetFullPivotPathFormatted(VisualizerMenuTemplatePivot pivot) =>
			string.Format(CultureInfo.InvariantCulture, "{0} > {1} > {2}",
				pivot.MenuItem.SU_BusinessContext,
				string.Join("/", new[] { pivot.MenuItem.SU_MenuPath, pivot.MenuItem.SU_MenuName }.Where(x => !x.IsEmpty)),
				pivot.SI_DocumentTitle);

		protected override void CheckSI_MenuTemplateFilter()
		{
			if (IsThereAnotherPivotWithTheSameFilter())
			{
				parent.SI_MenuTemplateFilterInfo.AddWarning(Res.GetString("ed8c35e7-a4ee-4169-a772-6071ce8fd28c", "There is another document with the same filter. It may cause a problem because only one form can be shown to the user at a time."));
			}
		}

		bool IsThereAnotherPivotWithTheSameFilter()
		{
			if (parent.MenuItem == null)
			{
				return false;
			}

			return parent.MenuItem.Documents
				.OfType<VisualizerMenuTemplatePivot>()
				.Any(pivot => pivot != parent && parent.SI_MenuTemplateFilter == pivot.SI_MenuTemplateFilter);
		}

		protected override void CheckSI_DocumentTitle()
		{
			base.CheckSI_DocumentTitle();

			if (!parent.SI_DocumentTitleInfo.HasErrors()
				&& (parent.Template?.IsBillOfLadingTemplate ?? false)
				&& !IsDocumentTitleSuitableForBillOfLading(parent.SI_DocumentTitle))
			{
				var errorMessage = Res.GetString("FF97AB8E-DE3A-4F7E-95C6-6CEF0D4AD40E", "Bills Of Lading document Titles can only be one on of the following: ORIGINAL and COPY.");
				parent.SI_DocumentTitleInfo.AddError(errorMessage);
			}
		}

		bool IsDocumentTitleSuitableForBillOfLading(ZString title)
		{
			return title.ToString().Equals("ORIGINAL", StringComparison.InvariantCultureIgnoreCase)
					|| title.ToString().Equals("COPY", StringComparison.InvariantCultureIgnoreCase);
		}

		public override void ValidateAll()
		{
			parent.ClearRowNotifications();
			base.ValidateAll();
			parent.ValidateFilterExpression(parent.SI_MenuTemplateFilter);
		}
	}
}
