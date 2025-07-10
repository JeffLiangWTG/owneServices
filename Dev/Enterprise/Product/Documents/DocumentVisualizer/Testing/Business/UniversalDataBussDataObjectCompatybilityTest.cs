using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class UniversalDataBussDataObjectCompatybilityTest : TestCase
	{
		public UniversalDataBussDataObjectCompatybilityTest()
		{
			supportedTypes = new Lazy<IEnumerable<Type>>(GetSupportedTypes);
		}

		readonly Lazy<IEnumerable<Type>> supportedTypes;

		public void TestSupportedTypesOnUniversalShipment()
		{
			var check = new HashSet<string>(CheckType(typeof(Shipment)));

			AssertMultilineASCIIEquals("UniversalShipment types supported by Form Builder",
				"",
				string.Join(System.Environment.NewLine, check));
		}

		public void TestSupportedTypesOnUniversalEvent()
		{
			var check = new HashSet<string>(CheckType(typeof(Event)));

			AssertMultilineASCIIEquals("UniversalEvent types supported by Form Builder",
				"",
				string.Join(System.Environment.NewLine, check));
		}

		public void TestSupportedTypesOnUniversalSchedule()
		{
			var check = new HashSet<string>(CheckType(typeof(Schedule)));

			AssertMultilineASCIIEquals("UniversalSchedule types supported by Form Builder",
				"",
				string.Join(System.Environment.NewLine, check));
		}

		#region Implementation

		IEnumerable<string> CheckType(Type type)
		{
			return CheckType(type, new HashSet<Type>(new[] { type }));
		}

		IEnumerable<string> CheckType(Type type, HashSet<Type> typesToSkip)
		{
			var result = new List<string>();

			foreach (var property in type.GetProperties())
			{
				if (!typesToSkip.Add(property.PropertyType))
				{
					continue;
				}

				var isNullable = property.PropertyType.IsGenericType
					&& property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

				if (isNullable)
				{
					if (!IsTypeSupported(property.PropertyType))
					{
						result.Add($"{type.Name}.{property.Name} has unsupported type of '{property.PropertyType.GetFormattedName()}'");
					}

					continue;
				}

				var elementType = GetCollectionType(type);

				if (elementType != null)
				{
					result.AddRange(CheckType(elementType, typesToSkip));
					continue;
				}

				result.AddRange(CheckType(property.PropertyType, typesToSkip));
			}

			return result;
		}

		Type GetCollectionType(Type type)
		{
			if (type.IsGenericType
				&& type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
				&& type.GetGenericArguments().Any())
			{
				return type.GetGenericArguments().First();
			}

			return null;
		}

		bool IsTypeSupported(Type type)
		{
			return supportedTypes.Value.Contains(type);
		}

		IEnumerable<Type> GetSupportedTypes()
		{
			yield return typeof(ZBool?);
			yield return typeof(ZByte?);
			yield return typeof(ZShort?);
			yield return typeof(ZInt?);
			yield return typeof(ZLong?);
			yield return typeof(ZDecimal?);
			yield return typeof(ZString?);
			yield return typeof(ZDate?);
			yield return typeof(ZDateTime?);
			yield return typeof(ZDateTimeOffset?);
			yield return typeof(UXmlDateTime?);
			yield return typeof(ZBlob?);

			yield return typeof(ZCodeMappedZString?);

			yield return typeof(InstructionType?);
			yield return typeof(ImportAction?);
			yield return typeof(PostingInstruction?);
			yield return typeof(CollectionContent?);
			yield return typeof(DateType?);
			yield return typeof(DataType?);
			yield return typeof(TransportMode?);
			yield return typeof(LegType?);
			yield return typeof(TimeUnit?);
			yield return typeof(UNDGState?);

			yield return typeof(TransactionType?);
			yield return typeof(PaymentOrReceiptType?);
			yield return typeof(InvoiceTermType?);
			yield return typeof(DebitCredit?);
			yield return typeof(RevenueRecognitionType?);
			yield return typeof(BankAccountType?);
			yield return typeof(CrewType?);
			yield return typeof(LocationOfGoodsType?);
			yield return typeof(TransportTypeCode?);

			yield return typeof(MessageNumberType?);

			yield return typeof(DirectionType?);
		}

		#endregion
	}
}
