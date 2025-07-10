using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using FlexCel.Core;

namespace Enterprise.RemotePrinting.Engine
{
	public abstract class BasePrinterForXLSFile : BasePrinter
	{
		protected BasePrinterForXLSFile(PrintEngineJob job)
			: base(job)
		{
			LeftMargin = job.LeftMargin;
			TopMargin = job.TopMargin;
			VerticalScale = job.VerticalScale;
			HorizontalScale = job.HorizontalScale;
			PrinterDriverTemplate = job.PrinterDriverTemplate;
			DeleteCompanyLogo = job.DeleteCompanyLogo;
		}

		readonly int LeftMargin;
		readonly int TopMargin;
		readonly decimal VerticalScale; //= 100
		readonly decimal HorizontalScale; //= 100
		readonly bool DeleteCompanyLogo;
		readonly byte[] PrinterDriverTemplate;

		public void ModifyFileIfChangesRequired(ExcelFile excelFile)
		{
			Argument.NotNull(excelFile, nameof(excelFile));
			if (excelFile.ObjectCount < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(excelFile));
			}

			if (NeedToModifyXlsFile)
			{
				ApplyPrintQueueScalingAndMargins(excelFile);
				ApplyPrinterDriverSettings(excelFile);
				RemoveCompanyLogoIfRequested(excelFile);
			}
		}

		#region ApplyPrinterDriverSettings

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ApplyPrinterDriverSettings(ExcelFile excelFile)
		{
			if (PrinterDriverTemplate.Length > 1)
			{
				Log("Applying printer driver settings to Xls file");

				var adapter = new PrinterDriverAdapter(excelFile);
				adapter.SetPrinterDriverSettings(PrinterDriverTemplate);
			}
		}

		#endregion

		protected bool NeedToModifyXlsFile
		{
			get { return DeleteCompanyLogo || Scale != 100 || HorizontalScale != 100 || VerticalScale != 100 || TopMargin != 0 || LeftMargin != 0 || PrinterDriverTemplate.Length > 0; }
		}

		#region Scaling And Margins

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ApplyPrintQueueScalingAndMargins(ExcelFile excelFile)
		{
			if (HorizontalScale != 100 || VerticalScale != 100 || TopMargin != 0 || LeftMargin != 0)
			{
				Log("Applying scaling and margins to Xls file");

				byte[] originalProperties = MakeAllImagesResizableAndReturnOriginalState(excelFile);
				try
				{
					if (VerticalScale != 100)
					{
						for (int row = 1; row < excelFile.RowCount; row++)
						{
							var rowHeight = LimitToRange((int)(excelFile.GetRowHeight(row) * VerticalScale / 100), 0, FlxConsts.MaxRowHeight);
							excelFile.SetRowHeight(row, rowHeight);
						}
					}

					if (HorizontalScale != 100)
					{
						for (int col = 2; col < excelFile.ColCount; col++)
						{
							excelFile.SetColWidth(col, (int)(excelFile.GetColWidth(col) * HorizontalScale / 100));
						}
					}

					if (TopMargin != 0)
					{
						var rowHeight = LimitToRange(excelFile.GetRowHeight(1) + TopMargin, 0, FlxConsts.MaxRowHeight);
						excelFile.SetRowHeight(1, rowHeight);
					}

					if (LeftMargin != 0)
					{
						excelFile.SetColWidth(2, excelFile.GetColWidth(2) + LeftMargin);
					}
				}
				finally
				{
					RestoreAllImagesSizeProperties(excelFile, originalProperties);
				}
			}
		}

		static byte[] MakeAllImagesResizableAndReturnOriginalState(ExcelFile excelFile)
		{
			excelFile.ActiveSheet = 1;
			var result = new byte[excelFile.ObjectCount];
			for (int i = 1; i <= result.Length; i++)
			{
				TClientAnchor anchor = excelFile.GetObjectAnchor(i);
				if (anchor != null)
				{
					result[i - 1] = (byte)anchor.AnchorType;
					anchor.AnchorType = TFlxAnchorType.MoveAndResize;
					excelFile.SetObjectAnchor(i, anchor);
				}
			}
			return result;
		}

		[SuppressMessage("Microsoft.Contracts", "Enum-46-0")]
		static void RestoreAllImagesSizeProperties(ExcelFile excelFile, byte[] originalSizeProperties)
		{
			Argument.NotNull(originalSizeProperties, nameof(originalSizeProperties));
			excelFile.ActiveSheet = 1;
			for (int i = 1; i <= originalSizeProperties.Length; i++)
			{
				TClientAnchor anchor = excelFile.GetObjectAnchor(i);
				if (anchor != null)
				{
					anchor.AnchorType = (TFlxAnchorType)originalSizeProperties[i - 1];
					excelFile.SetObjectAnchor(i, anchor);
				}
			}
		}

		#endregion

		#region Remove Company Logo

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void RemoveCompanyLogoIfRequested(ExcelFile excelFile)
		{
			if (DeleteCompanyLogo && excelFile == null)
			{
				throw new ArgumentException("Invalid argument.", nameof(excelFile));
			}

			if (DeleteCompanyLogo)
			{
				Log("Removing company logo from Xls file");

				for (int sheetNumber = 1; sheetNumber <= excelFile.SheetCount; sheetNumber++)
				{
					excelFile.ActiveSheet = sheetNumber;
					string[] logoNames = GetLogoObjectNames(excelFile);

					foreach (var logoName in logoNames)
					{
						if (logoName != null && ObjectExists(excelFile, logoName)) // object may be deleted already
						{
							RemoveAllObjectsByName(excelFile, logoName);
						}
					}
				}
			}
		}

		static string[] GetLogoObjectNames(ExcelFile excelFile)
		{
			var objectNames = new List<string>();

			for (int i = 1; i <= excelFile.ObjectCount; i++)
			{
				string name = excelFile.GetObjectName(i);
				if (name != null && name.ToUpper().EndsWith("LOGO"))
				{
					objectNames.Add(name);
				}
			}

			return objectNames.ToArray();
		}

		static void RemoveAllObjectsByName(ExcelFile excelFile, string objectName)
		{
			Argument.NotNull(objectName, nameof(objectName));
			int index = GetObjectIndex(excelFile, objectName);
			while (index > 0)
			{
				excelFile.DeleteObject(index);
				index = GetObjectIndex(excelFile, objectName);
			}
		}

		public static bool ObjectExists(ExcelFile excelFile, string objectName)
		{
			Argument.NotNull(excelFile, nameof(excelFile));
			Argument.NotNull(objectName, nameof(objectName));
			return GetObjectIndex(excelFile, objectName) > 0;
		}

		/// <summary>
		/// 1-based index of the object name in the excel sheet
		/// </summary>
		static int GetObjectIndex(ExcelFile excelFile, string objectName)
		{
			for (int i = 1; i <= excelFile.ObjectCount; i++)
			{
				var currentObjectName = excelFile.GetObjectName(i);
				if (currentObjectName != null && currentObjectName.ToUpper() == objectName.ToUpper())
				{
					return i;
				}
			}
			return 0;
		}

		static int LimitToRange(int value, int inclusiveMinimum, int inclusiveMaximum)
		{
			if (value < inclusiveMinimum)
			{
				return inclusiveMinimum;
			}
			if (value > inclusiveMaximum)
			{
				return inclusiveMaximum;
			}
			return value;
		}

		#endregion
	}
}
