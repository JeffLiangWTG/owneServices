using CargoWise.Types;

namespace Enterprise.Customs.GB.MCP.ServiceTasks
{
	/// <summary>
	/// Unique consignment number. This can be a 9-, 12- or 14-character number.
	/// NB!!! If the number is NOT 14 chars, it will be padded-out to 14 using trailing zeroes.
	/// For example, if the real number is 123456789 it will be sent as 12345678900000.
	/// If the real number is 123456789012 it will be sent as 12345679801200.
	/// </summary>
	public class UniqueConsignmentNumber
	{
		/*
		 *  The UCN consists of a 5-char ship landing ID, a 4-char container ID, and up to 5 chars of serial numbers
		 *  within that container.  So if the ship lands and it's the 23456th docking, and then we pull out thr 60th container,
		 *  the UCN would be:
		 *  234560060
		 *  or
		 *  23456006000000
		 *
		 * If then we have several LCLs on the container, the last numbers are used to show the serial number of the LCL.
		 * For example:
		 *  23456006000001
		 *
		 */

		readonly ZString _fourteenCharUcn;
		readonly ZString _shipIdAkaUvi;
		readonly ZString _containerSequence;
		readonly ZString _lclSuffix;

		public UniqueConsignmentNumber(ZString xsFourteenCharacterNumberInput)
		{
			this._fourteenCharUcn = xsFourteenCharacterNumberInput;
			this._shipIdAkaUvi = this._fourteenCharUcn.SubstringSafe(0, 5);
			this._containerSequence = this._fourteenCharUcn.SubstringSafe(5, 4);
			this._lclSuffix = this._fourteenCharUcn.SubstringSafe(9, 5);
		}

		public ZString Raw { get { return _fourteenCharUcn; } }

		/// <summary>
		/// Contains the UCN following the unusual rules of MCP
		/// </summary>
		public ZString ProperlyTruncatedUCN
		{
			get
			{
				ZString result = this._fourteenCharUcn;
				if (result.EndsWith("00"))
				{
					result = result.Left(12);
					if (result.EndsWith("000"))
					{
						result = result.Left(9);
					}
				}
				return result;
			}
		}

		public ZString UcnWithoutLclSuffix
		{
			get { return this._shipIdAkaUvi + this._containerSequence; }
		}

		public ZString ShipIdAkaUvi
		{
			get { return this._shipIdAkaUvi; }
		}
		public ZString ContainerSequence
		{
			get { return this._containerSequence; }
		}
		public ZString LclSuffix
		{
			get { return this._lclSuffix; }
		}
	}
}
