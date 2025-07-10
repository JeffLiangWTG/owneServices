using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	public class CACustomsWrapper : DocumentWrapper, ICACustomsWrapper
	{
		public static CACustomsWrapper New(BaseJobDeclaration declaration, BusinessObjectFactory factory)
		{
			return new CACustomsWrapper(declaration, factory);
		}

		public CACustomsWrapper(BaseJobDeclaration declaration, BusinessObjectFactory factory)
			: base((JobDeclaration)declaration, factory)
		{
			this.declaration = (JobDeclaration)declaration;
		}

		readonly JobDeclaration declaration;

		public JobDeclaration Declaration
		{
			get { return declaration; }
		}

		public IJobDeclaration DeclarationExposed
		{
			get { return declaration; }
		}

		public DocDeclaration DocDeclaration
		{
			get { return docDeclaration ?? (docDeclaration = DocDeclaration.New(Declaration, Factory)); }
		}
		DocDeclaration docDeclaration;

		void ICACustomsWrapper.RemoveUnrelatedReleaseStatuses(IReleaseStatus releaseStatus)
		{
			var releaseStatuses = DocDeclaration.ReleaseStatuses;
			if (releaseStatuses.Count > 1)
			{
				var docStatusToKeep = (from DocReleaseStatus status in releaseStatuses
									   where ((BusinessObject)status.WrappedObject).PK == releaseStatus.PK
									   select status).FirstOrDefault();
				if (docStatusToKeep != null)
				{
					releaseStatuses.RemoveAll();
					releaseStatuses.Add(docStatusToKeep);
				}
			}
		}

		BusinessObjectCollection ICACustomsWrapper.GetCAReleaseStatus()
		{
			return DocDeclaration.ReleaseStatuses;
		}

		ZString ICACustomsWrapper.GetCAPreviousCCN()
		{
			return DocDeclaration.PreviousCargoControlNumber;
		}

		ZString ICACustomsWrapper.GetCATransactionNo()
		{
			return Declaration.DeclarationNumber;
		}

		ZString ICACustomsWrapper.GetCargoControlNumberForCanada()
		{
			return DocDeclaration.CargoControlNumber;
		}

		ZString ICACustomsWrapper.GetPreviousCargoControlNumberForCanada()
		{
			return DocDeclaration.PreviousCargoControlNumber;
		}

		ZString ICACustomsWrapper.GetCACarrierName()
		{
			return DocDeclaration.CarrierName;
		}

		ZString ICACustomsWrapper.GetCAUSPortOfExit()
		{
			return DocDeclaration.USPortOfExit;
		}

		ZDateTime ICACustomsWrapper.GetDateOfFirstArrival()
		{
			return Declaration.JE_DateOfFirstArrival;
		}

		ZDateTime ICACustomsWrapper.GetWarehouseReleaseDate()
		{
			return Declaration.JE_WarehouseReleaseDate;
		}
	}
}
