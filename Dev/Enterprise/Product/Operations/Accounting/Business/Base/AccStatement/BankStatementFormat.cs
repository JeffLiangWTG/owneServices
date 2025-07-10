using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	public class BankStatementFormat : NonPersistentBusinessObject
	{
		public BankStatementFormat()
			: base()
		{
		}

		public BankStatementFormat(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Overrides
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			StatementFileFormatName = StatementFileFormatNames.NativeXML;
		}
		#endregion

		#region StatementFileFormatName

		[MaxLength(20)]
		[List("StatementFileFormats_List")]
		public ZString StatementFileFormatName
		{
			get { return fStatementFileFormatName; }
			set
			{
				if (fStatementFileFormatName != value)
				{
					CheckMaximumLength(StatementFileFormatNameInfo, value);
					SetNonPersistentPropertyValue(StatementFileFormatNameInfo, ref fStatementFileFormatName, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateStatementFileFormatName();
					}
					StatementFileFormatNameInfo.RefreshBinding();
				}
			}
		}
		ZString fStatementFileFormatName;

		public void ValidateStatementFileFormatName()
		{
			ClearAllNotifications();
		}

		public ZPropertyInfo StatementFileFormatNameInfo
		{
			get { return GetZPropertyInfo(nameof(StatementFileFormatName)); }
		}

		#endregion

		#region StatementFormat

		public StatementFileFormats StatementFileFormat
		{
			get { return StatementFileFormatNames.StatementFileFormat(StatementFileFormatName); }
		}

		#endregion

		#region StatementFileFormats_List

		public CodeDescriptionPairList StatementFileFormats_List
		{
			get
			{
				if (fStatementFileFormats_List == null)
				{
					fStatementFileFormats_List = new CodeDescriptionPairList();

					fStatementFileFormats_List.AddPair(StatementFileFormatNames.NativeXML, Res.GetString("6d737ff6-deb5-48e3-98ef-b70740e22ead", "ediDataInterface XML format"));
					fStatementFileFormats_List.AddPair(StatementFileFormatNames.NABAustralia, Res.GetString("cbfc283a-c328-4c9c-8995-005ce3c46c39", "NAB Australia statement format"));
					fStatementFileFormats_List.AddPair(StatementFileFormatNames.ANZNewZealand, Res.GetString("a8393202-fd23-4361-a7c2-1fb42b09339d", "ANZ New Zealand statement format"));
					fStatementFileFormats_List.AddPair(StatementFileFormatNames.WestpacNewZealand, Res.GetString("f1883693-ac59-4408-90e9-5a7743100e5d", "Westpac New Zealand statement format"));
				}

				return fStatementFileFormats_List;
			}
		}

		CodeDescriptionPairList fStatementFileFormats_List;

		#endregion

		#region Validation

		public BankStatementFormatValidation Validation
		{
			get { return new BankStatementFormatValidation(this); }
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class StatementFileFormatNames
		{
			public const string NativeXML = "XML";
			public const string NABAustralia = "NAB Australia";
			public const string ANZNewZealand = "ANZ New Zealand";
			public const string WestpacNewZealand = "Westpac New Zealand";

			public static StatementFileFormats StatementFileFormat(string statementFileFormatName)
			{
				StatementFileFormats result;
				switch (statementFileFormatName)
				{
					case NativeXML:
						result = StatementFileFormats.NativeXML;
						break;
					case NABAustralia:
						result = StatementFileFormats.NABAustralia;
						break;
					case ANZNewZealand:
						result = StatementFileFormats.ANZNewZealand;
						break;
					case WestpacNewZealand:
						result = StatementFileFormats.WestpacNewZealand;
						break;
					default:
						throw new ArgumentException("Invalid Statement File Format.");
				}
				return result;
			}
		}

		public enum StatementFileFormats
		{
			NativeXML,
			NABAustralia,
			ANZNewZealand,
			WestpacNewZealand
		}

		#endregion
	}
}
