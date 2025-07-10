using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class D4MessageInterpretationGenerator : UniversalEventMessageInterpretationGenerator
	{
		public D4MessageInterpretationGenerator(BusinessObjectFactory factory, EDIMessage message) : base(factory, message)
		{
		}

		public D4MessageInterpretationGenerator(BusinessObjectFactory factory, UniversalEvent universalEvent)
			: base(factory, universalEvent)
		{
		}

		internal new IEnumerable<D4PGADetail> PGADetails => PGADetailsBase.Cast<D4PGADetail>();

		protected override PGADetailBase CreatePGADetailCore(Context context)
		{
			return new D4PGADetail(context, factory);
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class D4PGADetail : PGADetailBase
		{
			public D4PGADetail(Context context, BusinessObjectFactory factory) : base(context, factory)
			{
			}
			protected override void PopulatePropertyValuesCore(List<Context> subContextCollection)
			{
				foreach (var subContext in subContextCollection)
				{
					var subContextType = subContext.Type;
					if (subContextType != null)
					{
						switch (subContextType.Type.GetValueOrDefault())
						{
							case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.Type:
								Type = subContext.Value.GetValueOrDefault();
								break;
							case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.Port:
								Port = subContext.Value.GetValueOrDefault();
								break;
							case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.Code:
								Code = subContext.Value.GetValueOrDefault();
								break;
							default:
								break;
						}

						var subLocation = CACSubLocation.Load(factory, Code);
						SubLocation = subLocation != null ? subLocation.Description : ZString.Empty;
					}
				}
			}

			#region Properties

			[ColumnName(3)]
			public ZString Type { get; private set; }

			[ColumnName(4)]
			public ZString Port { get; private set; }

			[ColumnName(5)]
			public ZString Code { get; private set; }

			[ColumnName(6)]
			public ZString SubLocation { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			protected override string Cation => Res.GetString("3c5f0dbf-9f0e-4e09-92d2-ecb929097362", "PGA DETAILS");

			protected override IEnumerable<string> Titles => PropertyNameProvider.GetColumnTitles<D4PGADetail>();

			protected override IEnumerable<object> Values => new object[]
						{
							PGA,
							Name,
							Type,
							Port,
							Code,
							SubLocation
						};
			#endregion
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class RelatedDocument : NonPersistentBusinessObject
		{
			#region Properties
			public ZString DocumentType { get; set; }

			public ZString DocumentNumber { get; set; }

			#endregion
		}
	}
}
