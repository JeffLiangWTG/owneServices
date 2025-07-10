using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[CodeProperty(CusTempStorageRegHeader.Schema.SRH_Reference)]
	public sealed class CusTempStorageRegHeader : EU.TemporaryStorage.Business.CusTempStorageRegHeader
		, Integration.Customs.DE.ICusTempStorageRegHeader
		, IRelatedJob
	{
		public CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CusTempStorageRegHeader Load(BusinessObjectFactory factory, string reference, string mrn = null)
		{
			CusTempStorageRegHeader result = null;
			if (factory != null && (!string.IsNullOrEmpty(reference) || !string.IsNullOrEmpty(mrn)))
			{
				result = factory.Load<CusTempStorageRegHeader>(GetLoadQuery(reference, mrn)).OrderBy(x => x.SRH_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("E0D48BEB-7088-451E-9E01-6501F8017BCB", Caption = "Registration Number")]
		public override ZString SRH_Reference
		{
			get
			{
				var result = base.SRH_Reference;
				if (!result.IsEmpty)
				{
					result = SumARegistrationNumberFormatter.Format(result);
				}

				return result;
			}
			set => base.SRH_Reference = value;
		}

		[ResourceStringData("F6CB858B-166D-4F6B-A19B-F906089A3CEE", Caption = "Customer Reference")]
		[ReadOnly(true)]
		public override ZString SRH_InternalReference
		{
			get => base.SRH_InternalReference;
			set => base.SRH_InternalReference = value;
		}

		public new CusTempStorageRegLineCollection CusTempStorageRegLines => (CusTempStorageRegLineCollection)base.CusTempStorageRegLines;

		internal static ZQuery GetLoadQuery(string reference, string mrn = null)
		{
			var referenceNumbers = new List<string>();

			if (!string.IsNullOrEmpty(reference))
			{
				referenceNumbers.Add(reference);
			}

			if (!string.IsNullOrEmpty(mrn))
			{
				referenceNumbers.Add(mrn);
			}

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, referenceNumbers);
			query.AddToFilter(CusTempStorageRegHeaderSchema.SRH_AppCode, TemporaryStorageApplicationCodeList.Codes.SumA);
			return query;
		}

		protected override ZString HumanReadableNameCore => SRH_Reference;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines() => new CusTempStorageRegLineCollection(this);

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups GetNewLookups() => new CusTempStorageRegHeaderLookups(this);

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderValidation GetNewValidation() => new CusTempStorageRegHeaderValidation(this);

		protected override Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);

		#region IEDocsProvider Members

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderDocumentSupporter GetNewDocumentSupporter() => new CusTempStorageRegHeaderDocumentSupporter(this);

		#endregion

		#region IRelatedJob Members

		public ZString JobNumber => SRH_Reference;

		public ZString JobDescription => HumanReadableName;

		public ZString JobStatus => ZString.Empty;

		public ControllerID ControllerID => ControllerIDs.Customs.DE.SumARegister;

		public Guid BusinessObjectPK => PK.ToGuid();

		#endregion
	}
}
