using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.Public.Glow.TestHelpers
{
	class RatingGeneratorForTests
	{
		readonly DbConnection connection;

		public RatingGeneratorForTests(DbConnection connection)
		{
			this.connection = connection;
		}

		public DataRow GetRateLineItem(Guid pk, string tableOrViewName)
		{
			var commandText = string.Format("SELECT * FROM {0} WHERE TM_PK = '{1}'", tableOrViewName, pk);
			var table = DataUtils.GetDataTableFromQuery(connection, commandText);

			if (table.Rows.Count != 1)
			{
				throw new InvalidOperationException("Expected row count to be 1 but was " + table.Rows.Count.ToString(CultureInfo.InvariantCulture) + ".");
			}

			return table.Rows[0];
		}

		public Guid NewRateLine()
		{
			var headerPK = Guid.NewGuid();
			var entryPK = Guid.NewGuid();
			var linePK = Guid.NewGuid();
			var dateTime = DateTime.Today.AddMonths(-6); // sql date type cannot use ztype

			var commandText = @"
DECLARE @companyPK UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);

INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteDate, TH_QuoteNumber, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
SELECT @headerPK, @companyPK, 'QTE', @dateTime, REPLACE(STR(ISNULL(MAX(CAST(TH_QuoteNumber as int)), 0) + 1, 20), SPACE(1), '0'), GetUtcDate(), '~BP', GetUtcDate(), '~BP' FROM dbo.RatingHeader

INSERT dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateCategory, TI_Mode, TI_SystemCreateTimeUtc, TI_SystemCreateUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser) VALUES (@entryPK, @headerPK, @companyPK, @dateTime, 'AIR', 'LSE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT dbo.RateLines (TL_PK, TL_TI, TL_AC, TL_SystemCreateTimeUtc, TL_SystemCreateUser, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_RX_NKCurrency)
SELECT TOP 1 @linePK, @entryPK, AC_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'USD' FROM dbo.AccChargeCode WHERE AC_GC = @companyPK";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@entryPK", SqlDbType.UniqueIdentifier, entryPK);
				command.AddParameter("@linePK", SqlDbType.UniqueIdentifier, linePK);
				command.AddParameter("@dateTime", SqlDbType.Date, dateTime);
				command.ExecuteNonQuery();
			}

			return linePK;
		}

		public Guid NewRateLineItem(Guid rateLinePK, string type, decimal value = 0, decimal breakAmount = 0, decimal flatAmount = 0, string text = "")
		{
			var itemPK = Guid.NewGuid();
			var commandText = "INSERT dbo.RateLineItems (TM_PK, TM_TL, TM_Type, TM_Value, TM_Break, TM_FlatAmount, TM_Text, TM_SystemCreateTimeUtc, TM_SystemCreateUser, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser) VALUES (@itemPK, @linePK, @type, @value, @break, @flat, @text, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@itemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.AddParameter("@linePK", SqlDbType.UniqueIdentifier, rateLinePK);
				command.AddParameterBasedOnDbColumn("@type", type, RateLineItemsSchema.TM_Type);
				command.AddParameter("@value", SqlDbType.Money, value);
				command.AddParameter("@break", SqlDbType.Money, breakAmount);
				command.AddParameter("@flat", SqlDbType.Money, flatAmount);
				command.AddParameterBasedOnDbColumn("@text", text, RateLineItemsSchema.TM_Text);
				command.ExecuteNonQuery();
			}

			return itemPK;
		}
	}
}
