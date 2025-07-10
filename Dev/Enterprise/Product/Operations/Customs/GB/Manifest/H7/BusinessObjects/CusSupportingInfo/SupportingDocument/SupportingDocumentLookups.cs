using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class SupportingDocumentLookups : EU.H7.Business.SupportingDocumentLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override ICollection CodeList
		{
			get
			{
				var importExportParent = Parent.ImportExportParent;
				if (importExportParent is BusinessObject businessObject && !businessObject.IsDeleted)
				{
					var direction = ((bool)importExportParent.IsExport && (bool)importExportParent.IsImport) ? "Both" : (importExportParent.IsExport ? "Export" : "Import");

					return Factory.GetSupportingDocumentList(Parent.ImportExportParent.DataGroupingCode, direction, Parent.ImportExportParent.Level, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				}

				return new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList ActionList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, ZDateTime.Now);

		public CodeDescriptionPairList AvailabilityList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, ZDateTime.Now);
	}
}
