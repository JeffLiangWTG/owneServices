function FindPort(portControlID, postalCodeControlID, cityControlID, stateControlID, countryControlID) {
    if ($(portControlID) && $(postalCodeControlID) && $(cityControlID) && $(stateControlID) && $(countryControlID)) {
        ExecuteServiceMethod("FindPort",
                             JSON.encode({ PortControlID: portControlID,
                                 PostalCode: $(postalCodeControlID).get('value'),
                                 City: $(cityControlID).get('value'),
                                 State: $(stateControlID).get('value'),
                                 Country: $(countryControlID).get('value')
                             }));
    }
}
