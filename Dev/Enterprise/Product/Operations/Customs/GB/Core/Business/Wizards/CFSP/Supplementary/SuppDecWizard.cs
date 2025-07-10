using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public class SuppDecWizard : AutoSuppDecWizard
	{
		public SuppDecWizard(JobDeclaration parentDeclaration)
		{
			this.parentDeclaration = parentDeclaration;
			base.NumberOfPackagesOfParentBox6 = this.parentDeclaration.JE_TotalNoOfPacks;
			base.NumberOfSiblingDeclarations = this.parentDeclaration.NumberOfLinkedSupplementaryDeclarations;
			base.NumberOfPackagesDeclaredOnSiblingDeclarations = this.parentDeclaration.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations;
			base.NumberOfPackagesRemainingOnSiblingDeclarations = this.parentDeclaration.NumberOfPackagesRemainingOnChildSupplementaryDeclarations;
			using (GetValidationSuspender())
			{
				NumberPackagesToDeclare = 0;
			}
			if (this.parentDeclaration.IsExport)
			{
				DeclarationType = ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration;
			}
		}

		[List(nameof(DeclarationTypeList))]
		public override ZString DeclarationType
		{
			get { return base.DeclarationType; }
			set { base.DeclarationType = value; }
		}

		[List(nameof(SupplementaryProcedureList))]
		public override ZString SupplementaryProcedure
		{
			get { return base.SupplementaryProcedure; }
			set { base.SupplementaryProcedure = value; }
		}

		public CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration, ImportSADDeclarationTypeList.Descriptions.ImportSupplementaryDeclaration);
				result.AddPair(ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse, ImportSADDeclarationTypeList.Descriptions.ImportSupplementaryWarehouse);
				result.AddPair(ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration, ExportSADDeclarationTypeList.Descriptions.ExportSupplementaryDeclaration);
				return result;
			}
		}

		public CodeDescriptionPairList SupplementaryProcedureList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("Y", "SDP - simplified declaration procedures");
				result.AddPair("Z", "EIDR - entry in declarant's records (ex LCP - local customs procedures)");
				return result;
			}
		}

		public override ZInt NumberPackagesToDeclare
		{
			get
			{
				return base.NumberPackagesToDeclare;
			}
			set
			{
				base.NumberPackagesToDeclare = value;
				NumberOfPackagesRemainingBalance = NumberOfPackagesRemainingOnSiblingDeclarations - NumberPackagesToDeclare;
			}
		}

		public override ZInt NumberOfPackagesRemainingBalance
		{
			get { return base.NumberOfPackagesRemainingBalance; }
			set
			{
				base.NumberOfPackagesRemainingBalance = value;
				if (OnBalanceChanged != null)
				{
					OnBalanceChanged(this, new EventArgsWithCounter(value));
				}
			}
		}

		public class EventArgsWithCounter : EventArgs
		{
			public readonly int Balance;

			public EventArgsWithCounter(ZInt value)
			{
				Balance = value;
			}
		}

		public event EventHandler OnBalanceChanged;
		readonly JobDeclaration parentDeclaration;
	}
}
