using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using _Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine
{
	public abstract class MenuCustomisationDocumentSupporter : DocumentSupporter
	{
		protected MenuCustomisationDocumentSupporter(MenuCustomisation parentBO)
			: base(parentBO)
		{
			ParentBO = parentBO;
		}
		internal readonly MenuCustomisation ParentBO;

		public override ZBool ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			switch (dataContext)
			{
				case _Constants.DataContext.MapGenericFreightJob:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(_Constants.DataContext.MapGenericFreightJob, ParentBO) };
					break;
			}

			foreach (DocumentWrapper wrapper in result)
			{
				IWantToKnowMyParentDocumentSupporter knowledgeableWrapper = wrapper as IWantToKnowMyParentDocumentSupporter;
				if (knowledgeableWrapper != null)
				{
					knowledgeableWrapper.SetParentDocumentSupporter(this);
				}
			}
			return result;
		}

		protected override _Constants.DataContext[] GetSupportedDataContexts()
		{
			return new _Constants.DataContext[] { _Constants.DataContext.MapGenericFreightJob };
		}

		public interface IWantToKnowMyParentDocumentSupporter
		{
			void SetParentDocumentSupporter(MenuCustomisationDocumentSupporter parentDocumentSupporter);
			MenuCustomisationDocumentSupporter ParentDocumentSupporter { get; }
		}

		MacroValueProviderMapCollection valueProviderMaps;
		public MacroValueProviderMapCollection MacroValueProviderMaps => valueProviderMaps ?? (valueProviderMaps = new ValueProviderMap().Macros);

		#region Adding .DummyBOWithManyDecimalAndTextFields for Testing/Debugging Purposes
#if DEBUG
		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.WantsBusinessObjectOfType(typeof(Testing.DummyBOWithFields)))
			{
				Testing.DummyBOWithFields result = Factory.New<Testing.DummyBOWithFields>();
				result.Collection.AddNew();
				return new IBODocDataProvider[] { BODocDataProvider.Get(result) };
			}
			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = base.GetSupportedBODataSources();
			result.AddRange(GetSupportedBODataSourcesFor(typeof(Testing.DummyBOWithFields)));
			return result;
		}
#endif
		#endregion
	}
}
