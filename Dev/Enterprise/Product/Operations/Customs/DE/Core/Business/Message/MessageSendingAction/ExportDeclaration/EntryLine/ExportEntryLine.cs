using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEntryLine : MessageSendingAction
	{
		public ExportEntryLine(CusEntryLine entryLine) : base(entryLine, x => string.Empty)
		{
			this.entryLine = entryLine;
		}

		public static class Schema
		{
			public const string LineNumber = nameof(ExportEntryLine.LineNumber);
			public const string Tariff = nameof(ExportEntryLine.Tariff);
			public const string Description = nameof(ExportEntryLine.Description);
		}

		[ResourceStringData("4BE958A3-3394-450E-9D39-8A9C432A1946", Caption = "Entry Line No.")]
		public ZShort LineNumber => entryLine.CL_LineNumber;

		public ZPropertyInfo LineNumberInfo => GetZPropertyInfo(Schema.LineNumber);

		public ZString Tariff => entryLine.RandomLine.JI_Tariff;

		public ZPropertyInfo TariffInfo => GetZPropertyInfo(Schema.Tariff);

		public ZString Description => entryLine.RandomLine.JI_Description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}

		readonly CusEntryLine entryLine;
	}
}
