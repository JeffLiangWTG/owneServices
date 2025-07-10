using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class PackageAndMarksAndNumbersInfo
	{
		public PackageAndMarksAndNumbersInfo(JobComInvoiceLine invoiceLine)
			: this(invoiceLine, ignoreBulkPackage: false)
		{
		}

		public PackageAndMarksAndNumbersInfo(JobComInvoiceLine invoiceLine, bool ignoreBulkPackage)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.ignoreBulkPackage = ignoreBulkPackage;
		}

		protected readonly JobComInvoiceLine invoiceLine;
		readonly bool ignoreBulkPackage;

		public ZString Package
		{
			get
			{
				if (package == null)
				{
					SetPackageAndMarksNumbersInfo();
				}
				return package;
			}
		}
		string package;

		public ZString MarksAndNumbers
		{
			get
			{
				if (marksAndNumbers == null)
				{
					SetPackageAndMarksNumbersInfo();
				}
				return marksAndNumbers;
			}
		}
		string marksAndNumbers;

		public ZBool ContainsBulkPackages
		{
			get
			{
				if (containsBulkPackages == null)
				{
					SetPackageAndMarksNumbersInfo();
				}
				return containsBulkPackages.Value;
			}
		}
		ZBool? containsBulkPackages;

		#region Implementation

		void SetPackageAndMarksNumbersInfo()
		{
			var packageInfoSb = new ZStringBuilder();
			var marksAndNumberSb = new ZStringBuilder();

			var invoiceLinePivots = invoiceLine.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>();
			containsBulkPackages = false;

			foreach (var pivot in invoiceLinePivots)
			{
				var package = pivot.Package;

				AppendPackageInfoIfNecessary(packageInfoSb, pivot, package);
				AppendMarksAndNosIfNoEmpty(marksAndNumberSb, package);
			}

			packageInfoSb.AppendIfBuilderIsNotEmpty(SectionSeparator);
			marksAndNumberSb.AppendIfBuilderIsNotEmpty(SectionSeparator);

			package = packageInfoSb.ToString();
			marksAndNumbers = marksAndNumberSb.ToString();
		}

		void AppendPackageInfoIfNecessary(ZStringBuilder packageInfoSb, Customs.Business.InvoiceLinePackagePivot pivot, Customs.Business.BasePackage package)
		{
			var packageType = GetPackageType(package);

			var isBulk = IsBulk(packageType);

			if (!ignoreBulkPackage || !isBulk)
			{
				packageInfoSb.AppendIfBuilderIsNotEmpty(ValueSeparator);
				packageInfoSb.Append(FormattableString.Invariant($" {pivot.CHC_NumberOfPacks} {packageType}").Trim());
			}

			containsBulkPackages |= isBulk;
		}

		protected virtual ZString GetPackageType(Customs.Business.BasePackage package) => package?.CW_PackType ?? ZString.Empty;

		void AppendMarksAndNosIfNoEmpty(ZStringBuilder marksAndNumberSb, Customs.Business.BasePackage package)
		{
			var marksAndNos = package?.CW_MarksAndNos ?? ZString.Empty;
			if (!marksAndNos.IsEmpty)
			{
				marksAndNumberSb.AppendIfBuilderIsNotEmpty(ValueSeparator);
				marksAndNumberSb.Append(FormattableString.Invariant($"{marksAndNos}"));
			}
		}

		bool IsBulk(ZString packageType) => bulkPackageTypes.Contains(packageType);

		#endregion

		readonly ImmutableArray<string> bulkPackageTypes = new string[]
		{
			RefCusCodeBulkPackageUnitType.BulkGas,
			RefCusCodeBulkPackageUnitType.BulkGrains,
			RefCusCodeBulkPackageUnitType.BulkLiquid,
			RefCusCodeBulkPackageUnitType.BulkLiquidGas,
			RefCusCodeBulkPackageUnitType.BulkNodules,
			RefCusCodeBulkPackageUnitType.BulkPowders,
			RefCusCodeBulkPackageUnitType.BulkScrap
		}.ToImmutableArray();

		const string ValueSeparator = ", ";
		const string SectionSeparator = ";";
	}
}
