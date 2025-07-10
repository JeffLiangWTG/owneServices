using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public class GridLayoutElement : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string ColumnNumber = "ColumnNumber";
			public const string HeaderText = "HeaderText";
		}

		#endregion

		public ZInt ColumnNumber
		{
			get { return fColumnNumber; }
			set
			{
				fColumnNumber = value;
				if (!IsValidationSuspended)
				{
					ValidateColumnNumber();
				}
				ColumnNumberInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ColumnNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ColumnNumber); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May only be required by developers")]
		public void ValidateColumnNumber()
		{
			ColumnNumberInfo.ClearAllNotifications();
			if (ColumnNumber < 0)
			{
				ColumnNumberInfo.AddError("Column Number should be greater or equal to zero");
			}
		}

		[MaxLength(50)]
		public ZString HeaderText
		{
			get { return fHeaderText; }
			set
			{
				if (fHeaderText != value)
				{
					CheckMaximumLength(HeaderTextInfo, value);
					fHeaderText = value;
					HeaderTextInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo HeaderTextInfo
		{
			get { return GetZPropertyInfo(Schema.HeaderText); }
		}

		ZInt fColumnNumber;
		ZString fHeaderText;
	}
}
