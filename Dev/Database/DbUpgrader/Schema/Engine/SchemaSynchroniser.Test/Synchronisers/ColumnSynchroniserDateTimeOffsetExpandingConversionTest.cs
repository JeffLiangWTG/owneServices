using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserDateTimeOffsetExpandingConversionTest : DateTimeOffsetConversionTestCase
	{
		protected override string CreateMainDbScript { get; } = "CREATE TABLE DataTest (PK uniqueidentifier NOT NULL, Data datetimeoffset(4) NULL, CONSTRAINT PK_TestColumnPreSynchroniser PRIMARY KEY NONCLUSTERED (PK));";
		protected override string CreateTemplateDbScript { get; } = "CREATE TABLE DataTest (PK uniqueidentifier NOT NULL, Data datetimeoffset(7) NULL, CONSTRAINT PK_TestColumnPreSynchroniser PRIMARY KEY NONCLUSTERED (PK));";
		protected override IReadOnlyList<Tuple<Guid, string, string>> Rows { get; } = Enumerable.Range(0, 35).Select(i => Tuple.Create(Guid.Parse($"00000000-0000-0000-0000-0000000000{i:D2}"), $"2018-05-18 13:03:{i:D2}.1234+11:12", $"2018-05-18 13:03:{i:D2}.1234000 +11:12")).ToArray();
	}
}
