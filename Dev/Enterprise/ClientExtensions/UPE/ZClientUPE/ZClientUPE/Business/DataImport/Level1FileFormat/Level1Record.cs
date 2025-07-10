using System.Text;

using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class Level1Record
	{
		public void AddRecordLines(ZString[] values)
		{
			foreach (ZString s in values)
			{
				AddRecordLine(s);
			}
		}

		public void AddRecordLine(ZString value)
		{
			string recordType = value.SubstringSafe(RecordLine.Constants.RecordType.Position, RecordLine.Constants.RecordType.Length);
			switch (recordType)
			{
				case RecordLine.Constants.RecordTypes._200000:
					_200000 = new _200000Line(value);
					break;
				case RecordLine.Constants.RecordTypes._202000:
					_202000 = new _202000Line(value);
					break;
				case RecordLine.Constants.RecordTypes._300000:
					_300000 = new _300000Line(value);
					break;
				case RecordLine.Constants.RecordTypes._400000:
					_400000 = new _400000Line(value);
					break;
				case RecordLine.Constants.RecordTypes._401000:
					_401000 = new _401000Line(value);
					break;
				case RecordLine.Constants.RecordTypes._900000:
					_900000 = new _900000Line(value);
					break;
				default:
					if (recordType.StartsWith(RecordLine.Constants.RecordTypes._500000Type))
					{
						_500000Lines.Add(new _500000Line(value));
					}
					else if (recordType.StartsWith(RecordLine.Constants.RecordTypes._600000Type))
					{
						_600000Lines.Add(new _600000Line(value));
					}
					break;
			}
		}

		public bool IsEmpty
		{
			get { return Is200000Empty && Is202000Empty; }
		}

		bool Is200000Empty
		{
			get
			{
				return _200000 == null || (string.IsNullOrEmpty(_200000.OriginPort.Trim()) && string.IsNullOrEmpty(_200000.OriginCountry.Trim()) &&
					string.IsNullOrEmpty(_200000.DestinationPort.Trim()) && string.IsNullOrEmpty(_200000.DestinationCountry.Trim()) &&
					_200000.GoodsValue == 0m && _200000.DimensionalWeight == 0m);
			}
		}

		bool Is202000Empty
		{
			get { return _202000 == null || _202000.PiecesManifested == 0 && _202000.Weight == 0m; }
		}

		public _200000Line _200000;
		public _202000Line _202000;
		public _300000Line _300000;
		public _400000Line _400000;
		public _401000Line _401000;
		public _900000Line _900000;

		public _500000LineCollection _500000Lines
		{
			get
			{
				if (f500000Lines == null)
				{
					f500000Lines = new _500000LineCollection();
				}
				return f500000Lines;
			}
		}
		_500000LineCollection f500000Lines;

		public _600000LineCollection _600000Lines
		{
			get
			{
				if (f600000Lines == null)
				{
					f600000Lines = new _600000LineCollection();
				}
				return f600000Lines;
			}
		}
		_600000LineCollection f600000Lines;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (_200000 != null)
			{
				stringBuilder.Append(_200000.Value);
				stringBuilder.Append("\n");
			}

			if (_202000 != null)
			{
				stringBuilder.Append(_202000.Value);
				stringBuilder.Append("\n");
			}

			if (_300000 != null)
			{
				stringBuilder.Append(_300000.Value);
				stringBuilder.Append("\n");
			}

			if (_400000 != null)
			{
				stringBuilder.Append(_400000.Value);
				stringBuilder.Append("\n");
			}

			if (_401000 != null)
			{
				stringBuilder.Append(_401000.Value);
				stringBuilder.Append("\n");
			}

			foreach (RecordLine line in _500000Lines)
			{
				stringBuilder.Append(line.Value);
				stringBuilder.Append("\n");
			}

			foreach (RecordLine line in _600000Lines)
			{
				stringBuilder.Append(line.Value);
				stringBuilder.Append("\n");
			}

			if (_900000 != null)
			{
				stringBuilder.Append(_900000.Value);
				stringBuilder.Append("\n");
			}

			return stringBuilder.ToString();
		}
	}
}
