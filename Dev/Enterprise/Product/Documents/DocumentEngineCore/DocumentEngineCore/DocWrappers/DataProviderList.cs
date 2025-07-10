using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore
{
	public class DataProviderList
	{
		public DataProviderList(params IBODocDataProvider[] dataProviders)
		{
			Argument.NotNull(dataProviders, "IBODocDataProvider[] dataProviders");
			if (dataProviders.Length < 1)
			{
				throw new ArgumentException("You must have at least on element in the parameter IBODocDataProvider[] dataProviders");
			}
			this.primaryProvider = dataProviders[0];
			this.dataProviders = new List<IBODocDataProvider>();
			foreach (var dataProvider in dataProviders)
			{
				Add(dataProvider);
			}
		}

		readonly IBODocDataProvider primaryProvider;
		readonly List<IBODocDataProvider> dataProviders;

		public IBODocDataProvider[] AllDataProviders
		{
			get { return dataProviders.ToArray(); }
		}

		public IBODocDataProvider PrimaryDataProvider
		{
			get { return primaryProvider; }
		}

		public void Add(IBODocDataProvider dataProvider)
		{
			if (dataProvider != null)
			{
				dataProviders.Add(dataProvider);
			}
		}

		public PrintCopyType PrintCopyType { get; set; } = PrintCopyType.ALL;
	}
}
