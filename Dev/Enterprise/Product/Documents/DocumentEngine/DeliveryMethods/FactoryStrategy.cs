using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	public abstract class FactoryStrategy
	{
		internal BusinessObjectFactory GetFactory()
		{
			return GetFactoryCore();
		}
		protected abstract BusinessObjectFactory GetFactoryCore();

		public void SaveChunk(BusinessObjectFactory factory)
		{
			SaveChunkCore(factory);
		}
		protected abstract void SaveChunkCore(BusinessObjectFactory factory);

		#region SaveInChunks

		public class SaveInChunks : FactoryStrategy
		{
			protected override BusinessObjectFactory GetFactoryCore()
			{
				return new BusinessObjectFactory() { NameForDebugging = "FactoryStrategy.SaveInChunks" };
			}

			protected override void SaveChunkCore(BusinessObjectFactory factory)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true);
			}
		}

		#endregion

		#region PopulateButDoNotSave

		public class PopulateButDoNotSave : FactoryStrategy
		{
			public PopulateButDoNotSave(BusinessObjectFactory factory)
			{
				fFactory = factory;
			}
			readonly BusinessObjectFactory fFactory;

			protected override BusinessObjectFactory GetFactoryCore()
			{
				return fFactory;
			}

			protected override void SaveChunkCore(BusinessObjectFactory factory)
			{
				// do nothing, we just want to populate but not save
			}
		}

		#endregion

	}
}
