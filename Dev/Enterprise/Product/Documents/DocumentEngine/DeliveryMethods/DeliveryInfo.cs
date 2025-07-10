using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Watermark = Enterprise.RemotePrinting.Engine.Watermark;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	public class DeliveryInfo
	{
		#region Delivery Formats Enum

		public enum DeliveryFormats
		{
			Document = 0,
			Report = 1,
			TIFF = 2,
			File = 3
		}

		#endregion

		public DeliveryInfo(DeliveryFormats deliveryFormat)
		{
			DeliveryFormat = deliveryFormat;
			this.FileContents = new MemoryStream();

			Name = "";
			AttachedFilename = "";
			EmailSubjectLine = "";
			EmailFromAddress = "";
			EmailSignature = "";
			RelatedBusinessContext = "";
			FileFormat = "";
			Copies = 1;
			JobSubmittedBy = GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
			LineSpacing = 1;
			HasDataForCurrentFileFormat = true;
		}

		#region File Contents

		public bool ShouldConvertFromExcel(OutputFormatType formatType)
		{
			return (formatType == OutputFormatType.FAX || formatType == OutputFormatType.PDF ||
					formatType == OutputFormatType.PDFA || formatType == OutputFormatType.TIF) &&
				   (FileFormat.Equals("XLS", StringComparison.OrdinalIgnoreCase) ||
					FileFormat.Equals("XLSX", StringComparison.OrdinalIgnoreCase)) &&
				   DeliveryFormat != DeliveryInfo.DeliveryFormats.File;
		}

		public void SetFileContents(Stream fileContents, string fileFormat)
		{
			Stream originalfFileContents = this.FileContents;

			this.FileContents = fileContents;
			this.FileFormat = fileFormat.ToUpperInvariant();

			if (originalfFileContents != null)
			{
				originalfFileContents.Close();
			}
		}

		public void ReleaseFileContentsWhenSafe()
		{
			if (fileContentsLockCounter == 0)
			{
				ReleaseFileContents();
			}
			else
			{
				releaseRequested = true;
			}
		}

		bool releaseRequested;

		void ReleaseFileContents()
		{
			releaseRequested = false;
			SetFileContents(null, string.Empty);
		}

		public IDisposable LockFileContents()
		{
			fileContentsLockCounter++;
			return new DisposableAction(() =>
			{
				fileContentsLockCounter--;
				if (fileContentsLockCounter == 0 && releaseRequested)
				{
					ReleaseFileContents();
				}
			});
		}

		int fileContentsLockCounter;

		#endregion

		#region Properties

		public string Name { get; set; }
		public List<SheetName> SheetNames { get; } = new List<SheetName>();
		public string AttachedFilename { get; set; }
		internal string FilePath { get; set; }
		public Stream FileContents { get; private set; }
		public string EmailSubjectLine { get; set; }
		public decimal LineSpacing { get; set; }
		public string EmailFromAddress { get; set; }
		public string EmailSignature { get; set; }
		public string RelatedBusinessContext { get; set; }
		public StmPrintQueue PrintQueue { get; set; }
		public short Copies { get; set; }
		public bool IsCoverSheet { get; set; }
		public ZString JobSubmittedBy { get; set; }
		public bool ShowDraftWatermark { get; set; }
		public bool IsLocalDocument { get; set; }
		public bool ShouldSignByPFX => SignBy == DocumentsSignBy.PFX;
		public string SignBy { get; set; } = DocumentsSignBy.NON;
		public string FileFormat { get; set; }
		public bool HasDataForCurrentFileFormat { get; set; }
		public bool HasMergeIntoPrimaryExcelTemplateFailed { get; set; }
		public bool AllowRawView { get; set; }
		public IWorkflowProvider ElectronicData { get; set; }

		public ZGuid DeliveryGroupID = ZGuid.Empty;

		public ZDateTime RunDateTime { get; set; }

		/// <summary>
		/// Report format means it is an excel file, able to be merged and/or delivered as another file format type (TIF, PDF, etc).
		/// </summary>
		public DeliveryFormats DeliveryFormat { get; set; }

		/// <summary>
		/// Trailing space after the page, in 24ths of an inch.
		/// </summary>
		public int TrailingSpace { get; set; }

		/// <summary>
		/// Gets or sets the PK of the business object to log against.
		/// </summary>
		public ZGuid BusinessObjectPk { get; set; }

		public ZGuid ParentGuid = ZGuid.Empty;
		public string ParentTableName = "";

		public string DocumentType = "";

		public DeliveryInstructions Instructions { get; set; }

		/// <summary>
		/// Whether or not this delivery info is able to be used as the basis for merging other documents. Only XLS documents can be merged, TIF files remain separate.
		/// </summary>
		public bool IsSuitableForMerge
		{
			get { return !IsCoverSheet && (DeliveryFormat == DeliveryInfo.DeliveryFormats.Document || DeliveryFormat == DeliveryInfo.DeliveryFormats.Report); }
		}

		public ZGuid ParentPivotPK { get; set; }

		public DocumentPack DocumentPack { get; set; }

		public ZString PDFEncryptionPassword { get; set; }

		public DocumentProtector Protector { get; set; }

		#endregion

		#region Watermark

		public Watermark Watermark => watermark ?? (watermark = GetWatermark());
		Watermark watermark;

		Watermark GetWatermark()
		{
			Watermark result = null;

			if (EnvProxy.Instance.IsProductionSystem)
			{
				if (!string.IsNullOrEmpty(CustomWatermarkText))
				{
					result = GetTextWatermark(CustomWatermarkText);
				}
				else if (ShowDraftWatermark)
				{
					var registryValue = DocumentsDataRegistry.Instance.Watermark.Value;

					if (DocumentsDataRegistry.Instance.Watermark.Value.UseTextWatermark)
					{
						result = GetTextWatermark(registryValue.TextWatermark);
					}
					else
					{
						result = new ImageWatermark(
							DocumentsDataRegistry.Instance.Watermark.Value.ImageWatermarkAsBytes,
							Watermark.GetHorizontalAlignment(registryValue.HorizontalAlignment),
							Watermark.GetVerticalAlignment(registryValue.VerticalAlignment),
							registryValue.HorizontalOffset,
							registryValue.VerticalOffset,
							registryValue.Rotation);
					}
				}
			}
			else
			{
				result = WatermarkHelper.GetNonCommercialUseWatermark();
			}

			return result;
		}

		TextWatermark GetTextWatermark(string text)
		{
			var registryValue = DocumentsDataRegistry.Instance.Watermark.Value;

			return new TextWatermark(
				text,
				Watermark.GetHorizontalAlignment(registryValue.HorizontalAlignment),
				Watermark.GetVerticalAlignment(registryValue.VerticalAlignment),
				registryValue.HorizontalOffset,
				registryValue.VerticalOffset,
				registryValue.Rotation,
				Color.FromArgb((int)registryValue.Opacity, 0, 0, 0),
				(NoResString)"Arial",
				registryValue.FontSize,
				FontStyle.Bold);
		}

		internal MultilingualString CustomWatermarkText { get; set; }

		#endregion
	}

	public class SheetName
	{
		public string StrictName { get; set; }
		public string EntireName { get; set; }

		public override string ToString()
		{
			return EntireName;
		}
	}
}
