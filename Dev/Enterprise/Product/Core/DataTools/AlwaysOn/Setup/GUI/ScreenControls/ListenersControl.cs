#define SuppressResourceStringsCheckRegion
#region SuppressResourceStringsCheckRegion

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class ListenersControl : UserControl, IParentFormHook // Cannot use ZUserControl because this is an external tool
	{
		bool newNameOK;
		bool newIPOK;
		bool newNetMaskOK;
		bool newPortOK = true;

		const int listenersNameMaxLength = 15;

		readonly Regex ipSegmentRegex;
		readonly Regex portRegex;
		readonly Regex listenersNameRegex;

		public ListenersControl()
		{
			InitializeComponent();

			ipSegmentRegex = new Regex("^\\d{1,3}$");
			portRegex = new Regex("^\\d{1,5}$");
			listenersNameRegex = new Regex("^[\\w-_]{1," + listenersNameMaxLength.ToString() + "}$");

			// Set all creation fields as wrong (since they are)
			nameTextBox.BackColor = MainForm.TextBoxErrorColor;
			// Except for the port. That has a default value
			portTextBox.Text = portTextBox.Tag.ToString();
			if (string.IsNullOrEmpty(portTextBox.Text))
			{
				portTextBox.Text = "1433";
			}
		}

		public IDbServerInstance DbServer { get; set; }
		public IAvailabilityGroup AvailabilityGroup { get; set; }
		IListener CurrentListener { get; set; }

		void RefreshListenersInfo()
		{
			uiControlForm.ShowMessage(Invariant($"Loading listeners for availability group {AvailabilityGroup.GroupName}"));// no translation needed

			try
			{
				CurrentListener = ListenerFactory.GetForGroup(DbServer, AvailabilityGroup);
				ipAddressesDataGridView.Rows.Clear();

				if (CurrentListener != null)
				{
					nameTextBox.Text = CurrentListener.DnsName;
					nameTextBox.Enabled = false;
					foreach (var ipAddress in CurrentListener.IpAddresses)
					{
						using (var newRow = new DataGridViewRow())
						{
							newRow.CreateCells(ipAddressesDataGridView);
							newRow.CreateCells(ipAddressesDataGridView);
							newRow.Cells[0].Value = ipAddress.IpAddress;
							newRow.Cells[1].Value = ipAddress.SubnetMask;
							newRow.ReadOnly = true;
							ipAddressesDataGridView.Rows.Add(newRow);
						}
					}

					portTextBox.Text = CurrentListener.Port.ToString();

					uiControlForm.ShowMessage("Listener loaded correctly");// no translation needed
					removeListenerButton.Enabled = true;
				}
				else
				{
					removeListenerButton.Enabled = false;
					ClearTextBoxes();
					nameTextBox.Enabled = true;
					uiControlForm.ShowMessage("No listener associated with this group");// no translation needed
				}
			}
			catch (SqlException ex)
			{
				uiControlForm.ShowMessage(Invariant($"Error loading listener:\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}"));// no translation needed
			}
		}

		void ClearTextBoxes()
		{
			nameTextBox.Text = string.Empty;

			portTextBox.Text = DbServer.ServerInfo.PortNumber == default ? "1433" : DbServer.ServerInfo.PortNumber.ToString();

			ipAddressesDataGridView.Rows.Clear();
		}

		void ToggleAddListenerButton()
		{
			if (newNameOK && newIPOK && newNetMaskOK && newPortOK)
			{
				confirmButton.Enabled = true;
			}
			else
			{
				confirmButton.Enabled = false;
			}
		}

		#region IParentFormHook members

		public void HookUiControlForm(IUiControlForm uiControlForm)
		{
			this.uiControlForm = uiControlForm;
			// -= Removes the event if it has been already added, this prevents multiple firing of the event
			VisibleChanged -= ListenersControl_VisibleChanged;
			VisibleChanged += ListenersControl_VisibleChanged;
		}
		IUiControlForm uiControlForm;

		#endregion // IParentFormHook members

		#region Event handlers

		void nameTextBox_TextChanged(object sender, EventArgs e)
		{
			var nameBox = (TextBox)sender;

			// Make sure this is no more than three numbers
			if (listenersNameRegex.IsMatch(nameBox.Text))
			{
				nameBox.BackColor = nameBox.Parent.BackColor;
				newNameOK = true;
			}
			else
			{
				nameBox.BackColor = MainForm.TextBoxErrorColor;
				newNameOK = false;
			}
			ToggleAddListenerButton();
		}

		bool IsValidIpSegment(string segment, bool isLast)
		{
			int.TryParse(segment, out var ipSegment);

			return
				ipSegmentRegex.IsMatch(segment) &&
				(isLast && (0 < ipSegment && ipSegment < 255) || !isLast && 0 <= ipSegment && ipSegment <= 255);
		}

		void ipAddressSegmentTextBox_TextChanged(object sender, EventArgs e)
		{
			if (sender is TextBox segmentBox)
			{
				segmentBox.BackColor = newIPOK == IsValidIpAddress(segmentBox.Text) ? segmentBox.Parent.BackColor : MainForm.TextBoxErrorColor;
				ToggleAddListenerButton();
			}
		}

		void netMaskSegmentTextBox_TextChanged(object sender, EventArgs e)
		{
			if (sender is TextBox segmentBox)
			{
				// Make sure this is no more than three numbers
				if (newNetMaskOK = IsValidSubnetMask(segmentBox.Text))
				{
					segmentBox.BackColor = segmentBox.Parent.BackColor;
				}
				else
				{
					segmentBox.BackColor = MainForm.TextBoxErrorColor;
				}

				ToggleAddListenerButton();
			}
		}

		void portTextBox_TextChanged(object sender, EventArgs e)
		{
			if (sender is TextBox portBox)
			{
				newPortOK = (portRegex.IsMatch(portBox.Text) && int.Parse(portBox.Text) <= 65535);

				portBox.BackColor = newPortOK ? portBox.Parent.BackColor : MainForm.TextBoxErrorColor;

				ToggleAddListenerButton();
			}
		}

		void portTextBox_Leave(object sender, EventArgs e)
		{
			if (sender is TextBox portBox && string.IsNullOrEmpty(portBox.Text))
			{
				portBox.BackColor = portBox.Parent.BackColor;
				newPortOK = true;

				portBox.Text = portBox.Tag.ToString();
			}
		}

		void confirmButton_Click(object sender, EventArgs e)
		{
			confirmButton.Enabled = false;

			uiControlForm.ShowMessage("Updating availability group's listener");// no translation needed

			if (string.IsNullOrEmpty(CurrentListener?.Server?.ServerInfo?.ServerAlias))
			{
				uiControlForm.ShowError("No listener server info specified");// no translation needed
				return;
			}

			try
			{
				using var scope = Program.SqlContextManager.NewExecutionScope();
				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(CurrentListener.Server.ServerInfo);
				ListenerFactory.Save(CurrentListener, sqlContext);

				uiControlForm.ShowMessage("Listener created");
				RefreshListenersInfo();
			}
			catch (SqlException ex)
			{
				uiControlForm.ShowMessage(Invariant($"Listener could not be updated:\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}"));
			}
			finally
			{
				ToggleAddListenerButton();
			}
		}

		void ListenersControl_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
				RefreshListenersInfo();
				addListenerGroupBox.Text = Invariant($"Listener for availability group {AvailabilityGroup.GroupName}:");
			}
		}

		void removeListenerButton_Click(object sender, EventArgs e)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();
			try
			{
				var message = $"Are you sure you want to remove listener \"{nameTextBox.Text}\"?\r\nAll saved IPs will also be removed";
				var userResponse = MessageDialog.Show(message, "Remove Listener", MessageBoxButtons.YesNo);

				if (userResponse == DialogResult.Yes)
				{
					ListenerFactory.Remove(CurrentListener);
				}
			}
			catch (InvalidOperationException ex)
			{
				uiControlForm.ShowMessage(Invariant($"Error removing listener\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}"));
			}
			finally
			{
				RefreshListenersInfo();
				ToggleAddListenerButton();
			}
		}

		#endregion

		bool IsValidIpAddressCell(DataGridViewCell ipCell)
		{
			if (ipCell.Value == null)
			{
				return false;
			}
			return IsValidIpAddress(ipCell.Value.ToString());
		}

		bool IsValidIpAddress(string address)
		{
			var segments = address.Split('.');
			var isAddressOk = segments.Length == 4;

			for (var i = 0; i < segments.Length; i++)
			{
				isAddressOk = isAddressOk && IsValidIpSegment(segments[i], i == segments.Length - 1);
			}
			return isAddressOk;
		}

		bool IsValidSubnetMaskCell(DataGridViewCell subnetCell)
		{
			if (subnetCell.Value == null)
			{
				return false;
			}
			return IsValidSubnetMask(subnetCell.Value.ToString());
		}

		bool IsValidSubnetMask(string subnet)
		{
			var segments = subnet.Split('.');
			var isAddressOk = segments.Length == 4;

			for (var i = 0; i < segments.Length; i++)
			{
				isAddressOk = isAddressOk && IsValidIpSegment(segments[i], false);
			}
			return isAddressOk;
		}

		private protected void validateListenerIpsButton_Click(object sender, EventArgs e)
		{
			if (CurrentListener == null)
			{
				CurrentListener = new Listener(nameTextBox.Text, int.Parse(portTextBox.Text, CultureInfo.InvariantCulture), DbServer, AvailabilityGroup);
			}
			CurrentListener.IpAddresses.Clear();

			if (ipAddressesDataGridView.RowCount == 1)
			{
				newIPOK = false;
				newNetMaskOK = false;
				uiControlForm.ShowError("There are no IPs specified for this listener");
				return;
			}
			newIPOK = true;
			newNetMaskOK = true;
			var errorMessage = string.Empty;
			for (var rowIndex = 0; rowIndex < ipAddressesDataGridView.RowCount - 1; rowIndex++)
			{
				var lastIpok = true;
				var lastNetMaskOk = true;

				var ipCell = ipAddressesDataGridView.Rows[rowIndex].Cells[0];
				var subnetCell = ipAddressesDataGridView.Rows[rowIndex].Cells[1];

				if (!IsValidIpAddressCell(ipCell))
				{
					lastIpok = false;
					errorMessage += Invariant($"IP address on line {rowIndex + 1} is invalid. {System.Environment.NewLine}");
				}
				if (!IsValidSubnetMaskCell(subnetCell))
				{
					lastNetMaskOk = false;
					errorMessage += Invariant($"Subnet mask on line {rowIndex + 1} is invalid. {System.Environment.NewLine}");
				}

				newIPOK = newIPOK && lastIpok;
				newNetMaskOK = newNetMaskOK && lastNetMaskOk;

				if (lastIpok && lastNetMaskOk)
				{
					var ip = ipCell.Value.ToString();
					var subnet = subnetCell.Value.ToString();
					CurrentListener.IpAddresses.Add(new ListenerIp(ip, subnet));
				}
			}

			if (newIPOK && newNetMaskOK)
			{
				uiControlForm.ShowMessage("Listener IPs validated");
			}
			else
			{
				uiControlForm.ShowMessage(errorMessage);
				uiControlForm.AppendMessage("Fix the above errors and click again the 'Validate Listener IPs' button");
			}

			ToggleAddListenerButton();
		}

		void ipAddressesDataGridView_Enter(object sender, EventArgs e)
		{
			uiControlForm.ShowMessage("Click 'Validate Listener IPs' button to validate the IPs\r\n\r\nPreviously saved IPs cannot be edited. Please remove and recreate the listener to re-enter them");
			confirmButton.Enabled = false;
		}
	}
}
#endregion
