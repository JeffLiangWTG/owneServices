using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public EMCSJobDeclarationDocumentSupporter(EMCSJobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		protected override IEnumerable<ZString> HideFilterCodeList => new ZString[] { EMCSJobDeclaration.EMCSApplicationCode };

		protected override bool HideFilter() => HideFilterCodeList.Contains(BaseJobDeclaration.JE_ApplicationCode);

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

		protected override DataContext[] GetSupportedDataContexts() => new DataContext[]
		{
			DataContext.EMCSDeclaration
		};

		protected override List<DataContextValue> GetSupportedBODataSources() => new ()
		{
			new DataContextValue(DeclarationDataContextValue)
		};

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new List<IBODocDataProvider>();
			if (dataContextValue.Equals(new DataContextValue(DeclarationDataContextValue)))
			{
				result.Add(CreateDeclarationWrapper());
			}
			return result.ToArray();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var wrappers = new List<DocumentWrapper>();

			switch (dataContext)
			{
				case DataContext.EMCSDeclaration:
					wrappers.Add(CreateDeclarationWrapper());
					break;
			}

			return (wrappers.Count > 0) ? wrappers.ToArray() : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		protected virtual ZString WrapperClassFullNamespace => "Enterprise.DocumentWrappers.Customs.EU.EMCS.EMCSDeclarationWrapper";

		protected virtual string WrapperClassAssemblyName => null;

		DocumentWrapper CreateDeclarationWrapper() => DocumentWrapperFactory.CreateWrapperWithoutException(WrapperClassFullNamespace, BusinessObject, WrapperClassAssemblyName);

		const string DeclarationDataContextValue = ".EMCSJobDeclaration";
	}
}
