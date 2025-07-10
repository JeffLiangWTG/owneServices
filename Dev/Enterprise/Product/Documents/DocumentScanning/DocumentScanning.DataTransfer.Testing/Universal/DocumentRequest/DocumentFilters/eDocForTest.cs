using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	internal class eDocForTest : IeDoc
	{
		public ZBool IsDeleted { get; set; }
		public ZBool IsPublished { get; set; }
		public ZBool IsSystemGenerated { get; }
		public ZDateTime DateAdded { get; set; }
		public ZDateTime LastEdited { get; set; }
		public ZString LastEditedUser { get; set; }
		public ZGuid UniqueKey { get; set; }
		public ZString Description { get; set; }
		public ZString DocType { get; set; }
		public ZString DocSource { get; set; }
		public ZString DocSourceDescription { get; set; }
		public ZString FileName { get; }
		public ZString DataType { get; }
		public ZString FileNameOnly { get; }
		public CodeDescriptionPairList DocType_List { get; }
		public BusinessObject ParentMain { get; set; }
		public ZString VisibleCompanyCode { get; set; }
		public ZString VisibleBranchCode { get; set; }
		public ZString VisibleDepartmentCode { get; set; }
		public ZBool IsCustomisableDocTypes { get; }
		public ZBlob ImageData { get; set; }
		public ZDecimal FileSizeInMB { get; set; }

		public void NotifyReadByUser()
		{
			throw new NotImplementedException();
		}

		public void SetValuesForTest(ZDateTime dateTime, ZString dataType)
		{
			throw new NotImplementedException();
		}

		public IDisposable OpenForEdit()
		{
			throw new NotImplementedException();
		}

		public Stream GetImageDataReader()
		{
			throw new NotImplementedException();
		}

		public void SetImageDataStream(Stream stream)
		{
			throw new NotImplementedException();
		}

		public string CreateReference()
		{
			throw new NotImplementedException();
		}

		public void Delete()
		{
			throw new NotImplementedException();
		}
	}
}
