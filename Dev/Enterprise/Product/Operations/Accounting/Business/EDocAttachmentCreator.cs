using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class EDocAttachementCreator<T> : AccPrintingUtility
		where T : BusinessObject, IDocManagerSupport
	{
		public EDocAttachementCreator(T bizo, string fileName, string refDocType, bool hideProgressWindow, bool shouldSaveDocManagerInfo = true)
			: base(bizo.Factory)
		{
			this.BizO = bizo;
			this.FileName = fileName;
			this.HideProgressWindow = hideProgressWindow;
			this.RefDocTypeCode = refDocType;
			this.ShouldSaveDocManagerInfo = shouldSaveDocManagerInfo;
		}

		protected override DeliveryInstructionDestination RunPrintSet(DocumentCommand command, AllowedDeliveryOptions options, ZGuid printQueuePK)
		{
			EDocCreatedUniqueKey = ZGuid.Empty;
			DeliveryInstructionDestination result = DeliveryInstructionDestination.None;

			using (DocumentPrintSet printSet = new DocumentPrintSet(command, null))
			{
				DirectoryInfo tempOutputDir = null;
				try
				{
					DeliveryInstructions instructions = new DeliveryInstructions();
					instructions.Destination = DeliveryInstructionDestination.Disk;
					instructions.IsDraft = true;
					instructions.OutputDirectory = Env.GetTempFileName();
					File.Delete(instructions.OutputDirectory);
					if (HideProgressWindow)
					{
						printSet.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;	//This will hide the progress window while delivering the document
					}
					tempOutputDir = Directory.CreateDirectory(instructions.OutputDirectory);
					printSet.Run(instructions);
					try
					{
						using (var stream = new MemoryStream(File.ReadAllBytes(tempOutputDir.GetFiles()[0].FullName)))
						{
							var pdfBytesAsArray = DocumentConverter.ConvertFromExcel(stream.ToArray(), null, OutputFormatType.PDF, null, false, 1.0m);
							EDocCreatedUniqueKey = BizO.DocManagerInfo.AddFileOrDocument(pdfBytesAsArray, FileName, RefDocTypeCode).UniqueKey;

							if (ShouldSaveDocManagerInfo)
							{
								BizO.DocManagerInfo.Save();
							}
						}

						result = instructions.Destination;
					}
					catch (IndexOutOfRangeException)
					{
						throw new ZCannotSaveException(Res.GetString("1c71fd0b-42b1-4474-a100-81363c4c3f7f",
@"Failed to save eDoc because the document {0} does not have any printable templates.
Please go to Documents -> Customize -> {0} -> Templates -> Templates Used and tick 'Print' for the relevant template.",
command.SU_MenuName), Res.GetString("8c574a9f-cf3b-483f-8a84-581d28a4e2cd", "Cannot Save eDocs"));
					}
				}
				finally
				{
					if (tempOutputDir != null)
					{
						tempOutputDir.Delete(true);
					}
				}

				return result;
			}
		}

		readonly T BizO;
		readonly string FileName;
		readonly bool HideProgressWindow;
		readonly string RefDocTypeCode;
		readonly bool ShouldSaveDocManagerInfo;
		public ZGuid EDocCreatedUniqueKey { get; private set; }
	}
}