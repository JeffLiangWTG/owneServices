using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
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
				var importExportParent = Parent.ImportExportParent as BusinessObject;
				if (importExportParent == null || importExportParent.IsDeleted)
				{
					return new CodeDescriptionPairList();
				}
				return Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services == Parent.ImportExportParent.DataGroupingCode ?
					Factory.GetSupportingDocumentList(Parent.ImportExportParent.DataGroupingCode, Parent.ParentDirection, UniversalReferenceConstants.RefCusCodeListLevelType.Both, includeParentDataGroupings: RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent) :
					Factory.GetSupportingDocumentList(Parent.ImportExportParent.DataGroupingCode, Parent.ParentDirection, Parent.ImportExportParent.Level, GetAdditionalCodeListAttributeFilters(), RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
			}
		}

		protected override IEnumerable<RefCusCodeListAttributeFilter> GetAdditionalCodeListAttributeFilters()
		{
			if (Parent.Parent is JobComInvoiceHeader)
			{
				yield return new RefCusCodeListAttributeFilter(
					RefCusCodeListAttributeTypes.Codes.Level,
					JoinCondition.And,
					true,
					values: UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
			}
		}

		public CodeDescriptionPairList ActionList
		{
			get
			{
				var supportingDocumentActionList = Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, ZDateTime.Now);

				if (Parent.RefCusCode == null)
				{
					return supportingDocumentActionList;
				}

				return Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GBSupportingDocumentLookups_ActionList_{0}", Parent.CSI_Code), () =>
				{
					var result = new CodeDescriptionPairList();
					var attributes = Parent.RefCusCode.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.ACTAV).Where(x => x.Length == 2);
					foreach (var action in attributes.Select(x => x.Right(1)))
					{
						if (!result.ContainsCode(action))
						{
							if (supportingDocumentActionList.ContainsCode(action))
							{
								result.Add(supportingDocumentActionList[action, StringComparison.OrdinalIgnoreCase]);
							}
							else
							{
								result.AddPair(action, "Action " + action);
							}
						}
					}

					result.Sort();
					return result;
				});
			}
		}

		public CodeDescriptionPairList AvailabilityList
		{
			get
			{
				var supportingDocumentAvailabilityList = Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, ZDateTime.Now);
				if (Parent.RefCusCode == null)
				{
					return supportingDocumentAvailabilityList;
				}

				return Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GBSupportingDocumentLookups_AvailabilityList_{0}", Parent.CSI_Code), () =>
				{
					var result = new CodeDescriptionPairList();
					var attributes = Parent.RefCusCode.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.ACTAV).Where(x => x.Length == 2);
					foreach (var avail in attributes.Select(x => x.Left(1)))
					{
						if (!result.ContainsCode(avail))
						{
							if (supportingDocumentAvailabilityList.ContainsCode(avail))
							{
								result.Add(supportingDocumentAvailabilityList[avail, StringComparison.OrdinalIgnoreCase]);
							}
							else
							{
								result.AddPair(avail, "Availability " + avail);
							}
						}
					}

					result.Sort();
					return result;
				});
			}
		}

		public override CodeDescriptionPairList UnitOfQuantityList => GBTaxLookupsCommon.GetUnitOfQuantityList(Parent.Factory, Parent.ImportExportParent.TrueCountryCode);
	}
}
