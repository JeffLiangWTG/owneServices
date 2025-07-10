using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[CodeAlive("Future use. The mechanism for creating additional tables in NCTS Pretty View without declaring table classes.")]
	public class NCTSPrettierTable : NCTSPrettierTableBase
	{
		public NCTSPrettierTable(
			ZString caption,
			(ZString Caption, ZString Width)[] columns,
			IEnumerable<object[]> rows,
			IReadOnlyCollection<(ZString Key, ZString Value)> additionalInfo = null,
			string border = "0",
			string width = "100%")
		{
			Caption = caption;
			if (columns.IsNullOrEmpty())
			{
				throw new ArgumentException($"{nameof(columns)} is null or empty", nameof(columns));
			}
			Columns = columns;
			if (rows.IsNullOrEmpty())
			{
				throw new ArgumentException($"{nameof(rows)} is null or empty", nameof(rows));
			}
			Rows = rows;
			AdditionalInfo = additionalInfo ?? Array.Empty<(ZString Key, ZString Value)>();
			Border = Argument.NotNull(border, nameof(border));
			Width = Argument.NotNull(width, nameof(width));
		}

		#region Implementation of INCTSPrettierTable

		public override ZString Caption { get; }

		public override ZString Border { get; }

		public override ZString Width { get; }

		public override IReadOnlyCollection<(ZString Key, ZString Value)> AdditionalInfo { get; }

		public override IReadOnlyCollection<(ZString Caption, ZString Attributes)> Columns { get; }

		#endregion

		protected override IEnumerable<object[]> Rows { get; }
	}
}
