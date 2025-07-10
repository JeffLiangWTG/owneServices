using System;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.Wow
{
	internal class ContainerManifestDummyLine : CsvRecord
	{
		internal ContainerManifestDummyLine(ContainerManifestCsvImportFile importFile, string line) : base(line, -1)
		{
		}

		public override string DisplayIdentifier
		{
			get
			{
				return "";
			}
		}

		public override bool SupportsUpdateBusinessData()
		{
			return false;
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			throw new NotSupportedException();
		}
	}
}
