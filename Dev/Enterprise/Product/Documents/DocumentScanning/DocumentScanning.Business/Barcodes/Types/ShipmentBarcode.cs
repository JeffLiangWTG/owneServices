using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Accepts a string [ROHCIVSYDMELH1234567890123456789012345] and splits it into its constituent parts
	/// </summary>
	public class ShipmentBarcode : ReferenceTypeBarcode
	{
		public ShipmentBarcode(DocumentFactory factory, ZString barcodeText)
			: base(factory, barcodeText)
		{
			if (IsValid)
			{
				DocManagerCode = Core.Constants.DocManagerCodes.Shipment;
				DocType = FullBarcodeText.Substring(4, 3);
				RefCode = (Shipment != null) ? (ZString)Shipment[JobShipmentSchema.JS_UniqueConsignRef] : ZString.Empty;
				RefPK = (Shipment != null) ? Shipment.PK : ZGuid.Empty;
			}
		}

		public ZString Company
		{
			get { return (IsValid) ? FullBarcodeText.Substring(1, 3) : ZString.Empty; }
		}

		public ZString Origin
		{
			get { return (IsValid) ? FullBarcodeText.Substring(7, 3) : ZString.Empty; }
		}

		public ZString Destination
		{
			get { return (IsValid) ? FullBarcodeText.Substring(10, 3) : ZString.Empty; }
		}

		public ZString HouseBill
		{
			get { return (IsValid) ? FullBarcodeText.Substring(13, FullBarcodeText.Length - 14) : ZString.Empty; }
		}

		/// <summary>
		/// Barcode must start with [ and end with ], have all 12 start characters 
		/// (3-letter Company, 3-letter DocType, 3-letter Origin, 3-letter Destination)
		/// and a minimum 3 letter length housebill.
		/// </summary>
#if DEBUG
		public
#else
		internal
#endif
		bool IsValid
		{
			get { return BarcodeHelper.IsShipmentBarcode(FullBarcodeText) && First12CharactersAreLetters && FullBarcodeText.Length > 17; }
		}
#if DEBUG
		public
#else
		internal
#endif
		bool First12CharactersAreLetters
		{
			get
			{
				bool returnValue = true;

				if (!(FullBarcodeText.Length > 13))
				{
					returnValue = false;
				}
				else
				{
					for (int i = 1; i < 13; i++)
					{
						if (!char.IsLetter(FullBarcodeText[i]))
						{
							returnValue = false;
						}
					}
				}
				return returnValue;
			}
		}

		/// <summary>
		/// Gets the shipment that this barcoded document should be allocated to. tries to match on IATA code first,
		/// then on last 3 letters of the 
		/// </summary>
#if DEBUG
		public
#else
		internal
#endif
		BusinessObject Shipment
		{
			get
			{
				if (fShipment == null && !HouseBill.IsEmpty)
				{
					fShipment = GetMatchingShipment(RefUNLOCOSchema.RL_IATA, RefUNLOCOSchema.RL_IATA);

					if (fShipment == null)
					{
						fShipment = GetMatchingShipment(RefUNLOCOSchema.RL_IATA, RefUNLOCOSchema.RL_Code);

						if (fShipment == null)
						{
							fShipment = GetMatchingShipment(RefUNLOCOSchema.RL_Code, RefUNLOCOSchema.RL_IATA);

							if (fShipment == null)
							{
								fShipment = GetMatchingShipment(RefUNLOCOSchema.RL_Code, RefUNLOCOSchema.RL_Code);
							}
						}
					}
				}
				return fShipment;
			}
		}
		BusinessObject fShipment;

		/// <summary>
		/// Looks for a shipment that matches the given housebill, origin and destination.
		/// Since it could match either the IATA or the last 3 letters of the code from the origin and
		/// destination, you must provide which field to search in (RefUNLOCOSchema.RL_Code or RefUNLOCOSchema.RL_IATA)
		/// </summary>
		/// <param name="UNLOCOFieldsToSearch">Should be either RefUNLOCOSchema.RL_Code, or RefUNLOCOSchema.RL_IATA</param>
		protected BusinessObject GetMatchingShipment(SchemaColumn originFieldToSearch, SchemaColumn destinationFieldToSearch)
		{
			ZQuery query = new ZQuery(JobShipmentSchema.JS_HouseBill, HouseBill);
			BusinessObject[] shipments = (BusinessObject[])MasterFactory.Load<Enterprise.Integration.Freight.ICommonShipment>(query);

			ZQuery originQuery = new ZQuery();
			originQuery.AddToFilter(JoinCondition.And, originFieldToSearch, SQLComparisonOperator.EndsWith, Origin);
			RefUNLOCO[] potentialOrigins = (RefUNLOCO[])MasterFactory.Load(typeof(RefUNLOCO), originQuery);

			ZQuery destinationQuery = new ZQuery();
			destinationQuery.AddToFilter(JoinCondition.And, destinationFieldToSearch, SQLComparisonOperator.EndsWith, Destination);
			RefUNLOCO[] potentialDestinations = (RefUNLOCO[])MasterFactory.Load(typeof(RefUNLOCO), destinationQuery);

			if (potentialOrigins.Length > 0 && potentialDestinations.Length > 0)
			{
				foreach (BusinessObject shipment in shipments)
				{
					foreach (RefUNLOCO originLoco in potentialOrigins)
					{
						foreach (RefUNLOCO destinationLoco in potentialDestinations)
						{
							if (shipment[JobShipmentSchema.JS_RL_NKOrigin].Equals(originLoco.Code) && shipment[JobShipmentSchema.JS_RL_NKDestination].Equals(destinationLoco.Code))
							{
								return shipment;
							}
						}
					}
				}
			}
			return null;
		}
	}
}
