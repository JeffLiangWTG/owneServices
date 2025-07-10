
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.BISI
{
	public abstract class RecordBodyLine : RecordLine
	{
		public RecordBodyLine(ILineKey lineKey)
		{
			this.LineKey = lineKey;
		}

		protected sealed override void AppendFields(ZStringBuilder builder)
		{
			AppendKeyFields(builder);
			AppendContentFields(builder);
		}

		void AppendKeyFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, LineKey.ShipmentRef, BaseLength.ShipmentNumber);
			AppendFixedLengthField(lineBuilder, GlbBranch.CurrentBranch.Country.Code, BaseLength.ImportCountry);
			AppendFixedLengthField(lineBuilder, LineType, BaseLength.LineType);
			AppendFixedLengthField(lineBuilder, LineKey.ImportDate, false, BaseLength.ImportDate);
			AppendFixedLengthField(lineBuilder, "ADD", BaseLength.ActionType);
		}

		#region Abstract

		protected abstract ZString LineType { get; }

		protected abstract void AppendContentFields(ZStringBuilder lineBuilder);

		#endregion

		#region Fields Length

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Class is inherited and cannot be static")]
		public class BaseLength
		{
			public const int ShipmentNumber = 11;
			public const int ImportCountry = 2;
			public const int LineType = 6;
			public const int ImportDate = 10;
			public const int ActionType = 3;
		}

		#endregion

		#region Line Types

		protected abstract class LineTypes
		{
			public const string ShipmentDetails = "100000";
			public const string ShipmentStatusDetails = "200000";
			public const string CountryDetails = "300000";
			public const string CommodityDetails = "400000";
			public const string ChargeDetails = "500000";
			public const string ImporterDetails = "600000";
			public const string BrokerageInfoDetails = "700000";
		}

		#endregion

		public readonly ILineKey LineKey;
	}
}
