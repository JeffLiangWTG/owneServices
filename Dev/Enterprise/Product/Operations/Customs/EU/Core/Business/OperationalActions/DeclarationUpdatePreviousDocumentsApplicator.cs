using System.Collections;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.OperationalActions
{
	public class DeclarationUpdatePreviousDocumentsApplicator : AutoDeclarationUpdatePreviousDocumentsApplicator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Name strings")]
		public DeclarationUpdatePreviousDocumentsApplicator(BusinessObjectFactory factory, ZString dataGroupingCode) : base("Previous Documents", factory)
		{
			DataGroupingCode = dataGroupingCode;
		}

		public static DeclarationUpdatePreviousDocumentsApplicator GetByDataGroupingCode(BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			DeclarationUpdatePreviousDocumentsApplicator result = null;
			if (!dataGroupingCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("DeclarationUpdatePreviousDocumentsApplicator");
				var objectHandle = (ObjectHandle)types[dataGroupingCode.ToString()];
				result = (DeclarationUpdatePreviousDocumentsApplicator)objectHandle?.GetObject(factory, dataGroupingCode);
			}
			return result ?? new DeclarationUpdatePreviousDocumentsApplicator(factory, dataGroupingCode);
		}

		[List(nameof(DocumentCodeList))]
		public override ZString DocumentCode { get => base.DocumentCode; set => base.DocumentCode = value; }

		[List(nameof(ClassCodeList))]
		public override ZString Class { get => base.Class; set => base.Class = value; }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new UpdatePreviousDocumentsOperationalActionRunner();
			foreach (var bo in targets)
			{
				if (bo is JobDeclaration declaration)
				{
					runner.UpdatePreviousDocuments(this, declaration, log);
				}
			}
		}

		public virtual CodeDescriptionPairList DocumentCodeList
		{
			get
			{
				return Factory.GetCachedValue("EUOperationalActions.PreviousDocumentCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, DataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, ZDateTime.Today));
					result.AddRangeOverwriteIfExists(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, DataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, ZDateTime.Today));
					result.SortByDescription();
					return result;
				});
			}
		}

		public virtual CodeDescriptionPairList ClassCodeList => Factory.GetCachedValue<PreviousDocumentClassList>();

		public ZString DataGroupingCode { get; }
	}
}
