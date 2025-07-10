using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationDeepCloneStrategy : JobDeclarationDeepCloneStrategy
	{
		public EMCSJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new EMCSJobComInvoiceHeaderDeepCopyStrategy((EMCSJobComInvoiceHeader)invoiceToClone, cloneType, (EMCSJobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newDec = (EMCSJobDeclaration)base.CloneInternal(args);
			var oldDec = ((EMCSJobDeclaration)bizObjToClone);
			CloneCustomsOffices(oldDec, newDec, args);
			return newDec;
		}

		void CloneCustomsOffices(EMCSJobDeclaration oldDec, EMCSJobDeclaration newDec, BusinessObjectCloneArgs args)
		{
			foreach (EuOfficeCode office in oldDec.CustomsOffices)
			{
				if (office.CY_Code == oldDec.CustomsOffices.DefaultPurposeCode)
				{
					newDec.CustomsOffices[0].CY_Data = office.CY_Data;
				}
				else
				{
					_ = newDec.CustomsOffices.AddNew(office.CY_Code, office.CY_Data);
				}
			}
		}

		protected override void DeepCopyCusInvPackCore(BaseJobDeclaration clonedResult)
		{
			var newDec = (EMCSJobDeclaration)clonedResult;
			var oldDec = ((EMCSJobDeclaration)bizObjToClone);

			foreach (EMCSPackage oldPackage in oldDec.EMCSPackages)
			{
				var newPackage = (EMCSPackage)oldPackage.Factory.New(oldPackage.GetType());

				using (newPackage.GetValidationSuspender())
				{
					using (newPackage.SuspendSettingHasChanges())
					{
						newPackage.B5_UnitType = oldPackage.B5_UnitType;
						newPackage.B5_UnitCount = oldPackage.B5_UnitCount;
						newPackage.B5_MarksAndNumbers = oldPackage.B5_MarksAndNumbers;
						newPackage.B5_SealNumber = oldPackage.B5_SealNumber;
						newPackage.B5_SealComment = oldPackage.B5_SealComment;
						newPackage.B5_ParentID = newDec.PK;
						newPackage.B5_ParentTableCode = oldPackage.B5_ParentTableCode;
						newDec.EMCSPackages.Add(newPackage);
					}
				}
			}
		}
	}
}
