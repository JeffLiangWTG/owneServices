using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierGuaranteesTable : NCTSPrettierTableBase
	{
		readonly IReadOnlyCollection<INCTSPrettierGuaranteeData> guarantees;

		public NCTSPrettierGuaranteesTable(IReadOnlyCollection<INCTSPrettierGuaranteeData> guarantees)
		{
			this.guarantees = Argument.NotNull(guarantees, nameof(guarantees));
		}

		public override ZString Caption => Res.GetString("021D513B-C70C-4155-BFD6-21A43C6E8D0C", "Guarantees");

		public override IReadOnlyCollection<(ZString Key, ZString Value)> AdditionalInfo => Array.Empty<(ZString Key, ZString Value)>();

		public override IReadOnlyCollection<(ZString Caption, ZString Attributes)> Columns => new (ZString Caption, ZString Attributes)[] {
			(Res.GetString("6B8F40B7-E096-4072-BD18-77B7131BE3A8", "Type"), (NoResString)"width=25%"),
			(Res.GetString("79D0BEB3-31CA-4A61-A80D-A847962BDC89", "GRN Number"), (NoResString)"width=25%"),
			(Res.GetString("5D38317C-1255-4EDE-9B23-0CCD61693680", "Other Number"), (NoResString)"width=25%"),
			(Res.GetString("18D0BA07-BC7B-42BE-A40F-5F66167AE395", "Amount"), (NoResString)"width=25%"),
		};

		protected override IEnumerable<object[]> Rows => guarantees.Select(x => new object[] { x.Type, x.GRN, x.OtherNumber, x.Amount + " " + x.Currency });
	}
}
