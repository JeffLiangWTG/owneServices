using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class DocumentTypeCodeDescriptionPairProvaderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new DocumentTypeCodeDescriptionPairProvider();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public override void TestIsReturningCorrectCollection()
		{
			ReadOnlyCodeDescriptionPairList docTypes = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();

			foreach (RefDocType refDocType in new BusinessObjectFactory().Load<RefDocType>(new ZQuery()))
			{
				if (refDocType.RT_IsActive
					&& (refDocType.RT_ReferenceType.In(new ZString[]
					{
						Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship,
						Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics,
						Enterprise.Core.Constants.ReferenceTypes.All
					})))
				{
					AssertCollectionContains("Expected doc type " + refDocType.RT_DocType, new CodeDescriptionPair(refDocType.RT_DocType.ToString(), refDocType.RT_Desc), docTypes);
				}
			}
		}

		public void TestGetCodeDescriptionPairList_ShouldNowReturnFreightDocTypesWhenPWModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var loadedDocTypePairs = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();

			var loadedDocTypes = new BusinessObjectFactory().Load<RefDocType>(new ZQuery());
			var docTypesFromLoadedDocTypePairs = new List<ZString>();

			docTypesFromLoadedDocTypePairs.AddRange(loadedDocTypes.Where(type => loadedDocTypePairs.ContainsCode(type.RT_DocType)).Select(type => type.RT_ReferenceType));

			AssertCollectionNotContains("We should load no doctypes with Freight reference types, and yet...",
				new ZString[] {
					Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship,
					Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics
				},
				docTypesFromLoadedDocTypePairs);
		}
	}
}
