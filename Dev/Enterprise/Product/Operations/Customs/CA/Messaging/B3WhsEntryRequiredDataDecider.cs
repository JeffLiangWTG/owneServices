using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Messaging
{
	#region B3WhsData enum

	enum B3WhsData
	{
		//IB3Header
		TransportMode,
		CarrierCodeAtImportation,
		PortOfDischarge,
		TotalValueForDuty,
		//IB3BRelease
		CargoControlNumber,
		DateOfRelease,
		//IB3SubHeader
		USPortOfExit,
		B3TimeLimits,
		FreightCharges,
		//IClassificationLine2
		CustomsDutyAmount,
		CustomsDutyRate,
		PreviousTransactionNumber,
		PreviousLineNumber,
		WeightInKGM,
		//IClassificationLine1
		ValueForTax,
		ExciseTaxAmount,
		GSTAmount,
		TRSNumber
	}

	#endregion

	class B3WhsEntryRequiredDataDecider
	{
		public B3WhsEntryRequiredDataDecider(IB3Header header)
		{
			this.header = header;
		}

		public bool IsRequired(B3WhsData data)
		{
			var index = B3EntryTypeList.WarehouseEntryTypes.IndexOf(header.B3TypeCode);
			return index < 0 || DecisionMatrix[(int)data, index];
		}

		public bool IsWarehouseEntry
		{
			get { return B3EntryTypeList.WarehouseEntryTypes.Contains(header.B3TypeCode); }
		}

		#region Implementation

		#region DecisionMatrix

		bool[,] DecisionMatrix
		{
			get
			{
				return decisionMatrix ?? (decisionMatrix = new[,]
				{
					//10     13     20     21     22     30
					{ true, false, false, false, false, false },	//TransportMode
					{ true, false, false, false, false, false },	//CarrierCodeAtImportation
					{ true, false, false, false, false, false },	//PortOfUnlading
					{ true, false, false, false, false, false },	//TotalValueForDuty
					{ true, false, false, false, false, false },	//CargoControlNumber
					{ true, false, true, false, false, false },	//DateOfRelease
					{ true, false, false, false, false, false },	//USPortOfExit
					{ true, true, true, false, false, true },		//B3TimeLimits
					{ true, false, false, false, false, false },	//FreightCharges
					{ true, true, true, false, false, false },	//CustomsDutyAmount
					{ true, true, true, false, false, false },	//CustomsDutyRate
					{ false, true, true, true, true, true },		//PreviousTransactionNumber
					{ false, true, true, true, true, true },		//PreviousLineNumber
					{ true, false, false, false, false, false },	//WeightInKGM
					{ true, true, true, false, false, false },	//ValueForTax
					{ true, true, true, false, false, false },	//ExciseTaxAmount
					{ true, true, true, false, false, false },	//GSTAmount
					{ true, true, true, false, false, false }		//TRSNumber
				});
			}
		}

		bool[,] decisionMatrix;

		#endregion

		readonly IB3Header header;

		#endregion
	}
}
