//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeRuleComponentLookups
//
//    This class should be used for overriding collections in AutoBarcodeRuleComponentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using Enterprise.BarcodeParsingEngine;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleComponentLookups : AutoBarcodeRuleComponentLookups
	{
		public BarcodeRuleComponentLookups(AutoBarcodeRuleComponent parent)
			: base(parent)
		{
		}

		new BarcodeRuleComponent Parent
		{
			get { return (BarcodeRuleComponent)base.Parent; }
		}

		#region ApplicationIdentifiers

		public ReadOnlyCodeDescriptionPairList ApplicationIdentifiers
		{
			get { return Factory.GetCachedValue("BarcodeRuleComponentLookups|ApplicationIdentifiers|" + Parent.IsGS1, () => GetApplicationIdentifiers()); }
		}

		ReadOnlyCodeDescriptionPairList GetApplicationIdentifiers()
		{
			ReadOnlyCodeDescriptionPairList result;

			if (Parent.IsGS1)
			{
				var applicationIdentifiers = new CodeDescriptionPairList();
				foreach (ApplicationIdentifier applicationIdentifier in WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value)
				{
					applicationIdentifiers.AddPair(applicationIdentifier.ApplicationID, applicationIdentifier.FullTitle);
				}

				result = applicationIdentifiers;
			}
			else
			{
				result = new ReadOnlyCodeDescriptionPairList();
			}

			return result;
		}

		#endregion

		#region FormatTypes

		public ReadOnlyCodeDescriptionPairList FormatTypes
		{
			get { return Factory.GetCachedValue("BarcodeRuleComponentLookups|FormatTypes|" + Parent.IsGS1, () => BarcodeFormatHelper.GetFormatTypes(Parent.IsGS1)); }
		}

		#endregion

		#region LengthTypes

		public LengthTypes LengthTypes
		{
			get { return Factory.GetCachedValue("BarcodeRuleComponentLookups|LengthTypes", () => new LengthTypes()); }
		}

		#endregion

		#region TargetFields

		public ReadOnlyCodeDescriptionPairList TargetFields
		{
			get { return Factory.GetCachedValue("BarcodeRuleComponentLookups|TargetFields|" + Parent.Module, () => GetTargetFields()); }
		}

		ReadOnlyCodeDescriptionPairList GetTargetFields()
		{
			var listWithIgnore = new CodeDescriptionPairList
			{
				new CodeDescriptionPair(BarcodeCaptureConstants.IgnoreCode,
					ResString.GetMultilingualString("BarcodeRuleComponentLookups|TargetFields|Ignore", "Ignore"))
			};

			listWithIgnore.AddRange(Factory.GetBarcodeParsingConsumerFromModuleCode(Parent.Module).TargetFields);
			return listWithIgnore;
		}

		#endregion
	}
}
